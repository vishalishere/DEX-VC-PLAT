using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Nethereum.Util;

namespace DecVCPlat.Common.Security
{
    /// <summary>
    /// Implementation of the security service that handles security-related functionalities
    /// </summary>
    public class SecurityService : ISecurityService
    {
        private readonly SecuritySettings _settings;
        private readonly IDistributedCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SecurityService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityService"/> class
        /// </summary>
        public SecurityService(
            IOptions<SecuritySettings> settings,
            IDistributedCache cache,
            IHttpClientFactory httpClientFactory,
            ILogger<SecurityService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<(bool IsValid, IEnumerable<string> ErrorMessages)> ValidatePasswordAsync(string password)
        {
            var errors = new List<string>();

            // Check password length
            if (string.IsNullOrEmpty(password) || password.Length < _settings.MinPasswordLength)
            {
                errors.Add($"Password must be at least {_settings.MinPasswordLength} characters long.");
            }

            // Check for uppercase letters
            if (_settings.RequireUppercase && !password.Any(char.IsUpper))
            {
                errors.Add("Password must contain at least one uppercase letter.");
            }

            // Check for lowercase letters
            if (_settings.RequireLowercase && !password.Any(char.IsLower))
            {
                errors.Add("Password must contain at least one lowercase letter.");
            }

            // Check for digits
            if (_settings.RequireDigit && !password.Any(char.IsDigit))
            {
                errors.Add("Password must contain at least one digit.");
            }

            // Check for special characters
            if (_settings.RequireSpecialCharacter && !password.Any(c => !char.IsLetterOrDigit(c)))
            {
                errors.Add("Password must contain at least one special character.");
            }

            // Check against common passwords
            if (_settings.CheckCommonPasswords)
            {
                var isCommon = await IsCommonPasswordAsync(password);
                if (isCommon)
                {
                    errors.Add("Password is too common and easily guessable.");
                }
            }

            return (errors.Count == 0, errors);
        }

        /// <inheritdoc />
        public async Task<string> HashPasswordAsync(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with PBKDF2
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Format: {algorithm}${iterations}${salt}${hash}
            return $"PBKDF2$10000${Convert.ToBase64String(salt)}${hashed}";
        }

        /// <inheritdoc />
        public async Task<bool> VerifyPasswordAsync(string password, string hash)
        {
            try
            {
                // Parse the hash string
                var parts = hash.Split('$');
                if (parts.Length != 4)
                {
                    return false;
                }

                var algorithm = parts[0];
                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var storedHash = parts[3];

                // Verify the algorithm
                if (algorithm != "PBKDF2")
                {
                    return false;
                }

                // Hash the input password with the same parameters
                string computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA512,
                    iterationCount: iterations,
                    numBytesRequested: 256 / 8));

                // Compare the computed hash with the stored hash
                return storedHash == computedHash;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password hash");
                return false;
            }
        }

        /// <inheritdoc />
        public string GenerateSecureToken(int length = 32)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        /// <inheritdoc />
        public async Task<string> EncryptAsync(string data, string purpose = null)
        {
            try
            {
                // Derive a key based on the master key and purpose
                var key = DeriveKey(_settings.EncryptionKey, purpose);

                // Generate a random IV
                byte[] iv = new byte[16];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }

                // Encrypt the data
                byte[] encrypted;
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new System.IO.MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new System.IO.StreamWriter(cs))
                        {
                            sw.Write(data);
                        }
                        encrypted = ms.ToArray();
                    }
                }

                // Combine IV and encrypted data
                var result = new byte[iv.Length + encrypted.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

                // Return as base64 string
                return Convert.ToBase64String(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting data");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> DecryptAsync(string encryptedData, string purpose = null)
        {
            try
            {
                // Derive the same key based on the master key and purpose
                var key = DeriveKey(_settings.EncryptionKey, purpose);

                // Decode the base64 string
                byte[] combined = Convert.FromBase64String(encryptedData);

                // Extract IV and encrypted data
                byte[] iv = new byte[16];
                byte[] encrypted = new byte[combined.Length - 16];
                Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(combined, iv.Length, encrypted, 0, encrypted.Length);

                // Decrypt the data
                string decrypted;
                using (var aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new System.IO.MemoryStream(encrypted))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var sr = new System.IO.StreamReader(cs))
                    {
                        decrypted = sr.ReadToEnd();
                    }
                }

                return decrypted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting data");
                throw;
            }
        }

        /// <inheritdoc />
        public string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Replace potentially dangerous HTML characters
            var sanitized = input
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("/", "&#x2F;");

            // Remove any script tags that might have been missed
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // Remove any on* attributes (e.g., onclick, onload)
            sanitized = Regex.Replace(sanitized, @"\s+on\w+\s*=\s*['""].*?['""]", "", RegexOptions.IgnoreCase);

            return sanitized;
        }

        /// <inheritdoc />
        public async Task<(bool IsValid, ClaimsPrincipal Principal)> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _settings.JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = _settings.JwtAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                // Check if token is revoked
                var tokenId = GetTokenIdFromJwt(token);
                if (!string.IsNullOrEmpty(tokenId))
                {
                    var isRevoked = await IsTokenRevokedAsync(tokenId);
                    if (isRevoked)
                    {
                        return (false, null);
                    }
                }

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                return (true, principal);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return (false, null);
            }
        }

        /// <inheritdoc />
        public async Task<string> GenerateJwtTokenAsync(string userId, string email, IEnumerable<string> roles, int expirationMinutes = 60)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var token = new JwtSecurityToken(
                issuer: _settings.JwtIssuer,
                audience: _settings.JwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc />
        public async Task<bool> VerifyCaptchaAsync(string captchaResponse, string remoteIp)
        {
            try
            {
                if (string.IsNullOrEmpty(captchaResponse))
                {
                    return false;
                }

                var client = _httpClientFactory.CreateClient();
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "secret", _settings.RecaptchaSecretKey },
                    { "response", captchaResponse },
                    { "remoteip", remoteIp }
                });

                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                var captchaResult = JsonSerializer.Deserialize<RecaptchaResponse>(responseContent);

                return captchaResult?.Success ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying CAPTCHA");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsSuspiciousIpAsync(string ipAddress)
        {
            try
            {
                // Check if IP is in the blocklist cache
                var isBlocked = await _cache.GetStringAsync($"blocked_ip:{ipAddress}");
                if (!string.IsNullOrEmpty(isBlocked))
                {
                    return true;
                }

                // Check against IP reputation service if configured
                if (!string.IsNullOrEmpty(_settings.IpReputationServiceUrl))
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.GetAsync($"{_settings.IpReputationServiceUrl}?ip={ipAddress}");
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var reputationResult = JsonSerializer.Deserialize<IpReputationResponse>(content);
                        
                        if (reputationResult?.Score > _settings.IpReputationThreshold)
                        {
                            // Cache the result
                            await _cache.SetStringAsync(
                                $"blocked_ip:{ipAddress}",
                                "true",
                                new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
                                });
                            
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking IP reputation for {IpAddress}", ipAddress);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task RecordFailedLoginAttemptAsync(string username, string ipAddress)
        {
            try
            {
                // Increment username-based counter
                var usernameKey = $"failed_login:{username}";
                var usernameCount = await _cache.GetStringAsync(usernameKey);
                int count = string.IsNullOrEmpty(usernameCount) ? 1 : int.Parse(usernameCount) + 1;
                
                await _cache.SetStringAsync(
                    usernameKey,
                    count.ToString(),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.LoginThrottlingResetMinutes)
                    });

                // Increment IP-based counter
                var ipKey = $"failed_login_ip:{ipAddress}";
                var ipCount = await _cache.GetStringAsync(ipKey);
                int ipAttempts = string.IsNullOrEmpty(ipCount) ? 1 : int.Parse(ipCount) + 1;
                
                await _cache.SetStringAsync(
                    ipKey,
                    ipAttempts.ToString(),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.LoginThrottlingResetMinutes)
                    });

                // If IP has too many failed attempts, block it temporarily
                if (ipAttempts >= _settings.MaxFailedAttemptsBeforeIpBlock)
                {
                    await _cache.SetStringAsync(
                        $"blocked_ip:{ipAddress}",
                        "true",
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_settings.IpBlockDurationMinutes)
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording failed login attempt for {Username} from {IpAddress}", username, ipAddress);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ShouldThrottleLoginAttemptsAsync(string username, string ipAddress)
        {
            try
            {
                // Check if IP is blocked
                var isBlocked = await _cache.GetStringAsync($"blocked_ip:{ipAddress}");
                if (!string.IsNullOrEmpty(isBlocked))
                {
                    return true;
                }

                // Check username-based attempts
                var usernameKey = $"failed_login:{username}";
                var usernameCount = await _cache.GetStringAsync(usernameKey);
                if (!string.IsNullOrEmpty(usernameCount) && int.Parse(usernameCount) >= _settings.MaxFailedLoginAttempts)
                {
                    return true;
                }

                // Check IP-based attempts
                var ipKey = $"failed_login_ip:{ipAddress}";
                var ipCount = await _cache.GetStringAsync(ipKey);
                if (!string.IsNullOrEmpty(ipCount) && int.Parse(ipCount) >= _settings.MaxFailedAttemptsPerIp)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking login throttling for {Username} from {IpAddress}", username, ipAddress);
                return false;
            }
        }

        /// <inheritdoc />
        public bool IsValidBlockchainAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            // Use Nethereum's address validation
            return AddressUtil.Current.IsValidEthereumAddressHexFormat(address);
        }

        /// <inheritdoc />
        public async Task StorePrivateKeyAsync(string userId, string privateKey)
        {
            try
            {
                // Encrypt the private key with a purpose specific to the user
                var encryptedKey = await EncryptAsync(privateKey, $"user_private_key:{userId}");
                
                // Store in the distributed cache with a long expiration
                await _cache.SetStringAsync(
                    $"private_key:{userId}",
                    encryptedKey,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(_settings.PrivateKeyStorageDays)
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error storing private key for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> RetrievePrivateKeyAsync(string userId)
        {
            try
            {
                // Get the encrypted key from cache
                var encryptedKey = await _cache.GetStringAsync($"private_key:{userId}");
                if (string.IsNullOrEmpty(encryptedKey))
                {
                    return null;
                }
                
                // Decrypt with the same purpose
                return await DecryptAsync(encryptedKey, $"user_private_key:{userId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving private key for user {UserId}", userId);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Checks if a password is in the list of common passwords
        /// </summary>
        private async Task<bool> IsCommonPasswordAsync(string password)
        {
            // This would typically check against a database or API of common passwords
            // For simplicity, we'll check against a small hardcoded list
            var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "password", "123456", "qwerty", "admin", "welcome",
                "password123", "abc123", "letmein", "monkey", "1234567890"
            };

            return commonPasswords.Contains(password);
        }

        /// <summary>
        /// Derives a key from the master key and purpose
        /// </summary>
        private byte[] DeriveKey(string masterKey, string purpose)
        {
            // If purpose is provided, use it as additional entropy
            var salt = string.IsNullOrEmpty(purpose)
                ? new byte[0]
                : Encoding.UTF8.GetBytes(purpose);

            // Use PBKDF2 to derive a key
            return KeyDerivation.Pbkdf2(
                password: masterKey,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 32); // 256 bits
        }

        /// <summary>
        /// Extracts the token ID (jti claim) from a JWT token
        /// </summary>
        private string GetTokenIdFromJwt(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
                    return jtiClaim?.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting token ID from JWT");
            }

            return null;
        }

        /// <summary>
        /// Checks if a token has been revoked
        /// </summary>
        private async Task<bool> IsTokenRevokedAsync(string tokenId)
        {
            var revoked = await _cache.GetStringAsync($"revoked_token:{tokenId}");
            return !string.IsNullOrEmpty(revoked);
        }

        #endregion

        #region Private Classes

        /// <summary>
        /// Response from reCAPTCHA verification
        /// </summary>
        private class RecaptchaResponse
        {
            public bool Success { get; set; }
            public string[] ErrorCodes { get; set; }
        }

        /// <summary>
        /// Response from IP reputation service
        /// </summary>
        private class IpReputationResponse
        {
            public int Score { get; set; }
            public string Category { get; set; }
        }

        #endregion
    }
}
