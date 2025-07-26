using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DecVCPlat.Common.Security
{
    /// <summary>
    /// Interface for security service that handles security-related functionalities
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// Validates a password against security requirements
        /// </summary>
        /// <param name="password">The password to validate</param>
        /// <returns>Validation result with success flag and any error messages</returns>
        Task<(bool IsValid, IEnumerable<string> ErrorMessages)> ValidatePasswordAsync(string password);
        
        /// <summary>
        /// Hashes a password using a secure algorithm
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        Task<string> HashPasswordAsync(string password);
        
        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="hash">The hash to verify against</param>
        /// <returns>True if the password matches the hash, false otherwise</returns>
        Task<bool> VerifyPasswordAsync(string password, string hash);
        
        /// <summary>
        /// Generates a secure random token
        /// </summary>
        /// <param name="length">The length of the token</param>
        /// <returns>The generated token</returns>
        string GenerateSecureToken(int length = 32);
        
        /// <summary>
        /// Encrypts sensitive data
        /// </summary>
        /// <param name="data">The data to encrypt</param>
        /// <param name="purpose">The purpose of encryption (used for key derivation)</param>
        /// <returns>The encrypted data</returns>
        Task<string> EncryptAsync(string data, string purpose = null);
        
        /// <summary>
        /// Decrypts sensitive data
        /// </summary>
        /// <param name="encryptedData">The encrypted data</param>
        /// <param name="purpose">The purpose of encryption (used for key derivation)</param>
        /// <returns>The decrypted data</returns>
        Task<string> DecryptAsync(string encryptedData, string purpose = null);
        
        /// <summary>
        /// Sanitizes user input to prevent XSS attacks
        /// </summary>
        /// <param name="input">The input to sanitize</param>
        /// <returns>The sanitized input</returns>
        string SanitizeInput(string input);
        
        /// <summary>
        /// Validates a JWT token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <returns>Validation result with claims principal if valid</returns>
        Task<(bool IsValid, ClaimsPrincipal Principal)> ValidateTokenAsync(string token);
        
        /// <summary>
        /// Generates a JWT token for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="email">The user email</param>
        /// <param name="roles">The user roles</param>
        /// <param name="expirationMinutes">Token expiration in minutes</param>
        /// <returns>The generated JWT token</returns>
        Task<string> GenerateJwtTokenAsync(string userId, string email, IEnumerable<string> roles, int expirationMinutes = 60);
        
        /// <summary>
        /// Verifies a CAPTCHA response
        /// </summary>
        /// <param name="captchaResponse">The CAPTCHA response from the client</param>
        /// <param name="remoteIp">The remote IP address</param>
        /// <returns>True if the CAPTCHA is valid, false otherwise</returns>
        Task<bool> VerifyCaptchaAsync(string captchaResponse, string remoteIp);
        
        /// <summary>
        /// Checks if an IP address is suspicious (e.g., known malicious, excessive requests)
        /// </summary>
        /// <param name="ipAddress">The IP address to check</param>
        /// <returns>True if the IP is suspicious, false otherwise</returns>
        Task<bool> IsSuspiciousIpAsync(string ipAddress);
        
        /// <summary>
        /// Records a failed login attempt for rate limiting
        /// </summary>
        /// <param name="username">The username that failed to login</param>
        /// <param name="ipAddress">The IP address that attempted the login</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task RecordFailedLoginAttemptAsync(string username, string ipAddress);
        
        /// <summary>
        /// Checks if login attempts should be throttled for a user or IP
        /// </summary>
        /// <param name="username">The username attempting to login</param>
        /// <param name="ipAddress">The IP address attempting the login</param>
        /// <returns>True if login attempts should be throttled, false otherwise</returns>
        Task<bool> ShouldThrottleLoginAttemptsAsync(string username, string ipAddress);
        
        /// <summary>
        /// Validates a blockchain wallet address
        /// </summary>
        /// <param name="address">The wallet address to validate</param>
        /// <returns>True if the address is valid, false otherwise</returns>
        bool IsValidBlockchainAddress(string address);
        
        /// <summary>
        /// Securely stores a private key with encryption
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="privateKey">The private key to store</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task StorePrivateKeyAsync(string userId, string privateKey);
        
        /// <summary>
        /// Retrieves a securely stored private key
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The decrypted private key</returns>
        Task<string> RetrievePrivateKeyAsync(string userId);
    }
}
