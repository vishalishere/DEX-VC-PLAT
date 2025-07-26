namespace DecVCPlat.Common.Security
{
    /// <summary>
    /// Settings for security-related functionalities
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Minimum password length
        /// </summary>
        public int MinPasswordLength { get; set; } = 8;
        
        /// <summary>
        /// Whether passwords require an uppercase letter
        /// </summary>
        public bool RequireUppercase { get; set; } = true;
        
        /// <summary>
        /// Whether passwords require a lowercase letter
        /// </summary>
        public bool RequireLowercase { get; set; } = true;
        
        /// <summary>
        /// Whether passwords require a digit
        /// </summary>
        public bool RequireDigit { get; set; } = true;
        
        /// <summary>
        /// Whether passwords require a special character
        /// </summary>
        public bool RequireSpecialCharacter { get; set; } = true;
        
        /// <summary>
        /// Whether to check passwords against a list of common passwords
        /// </summary>
        public bool CheckCommonPasswords { get; set; } = true;
        
        /// <summary>
        /// Key used for encryption/decryption
        /// </summary>
        public string EncryptionKey { get; set; }
        
        /// <summary>
        /// JWT issuer
        /// </summary>
        public string JwtIssuer { get; set; }
        
        /// <summary>
        /// JWT audience
        /// </summary>
        public string JwtAudience { get; set; }
        
        /// <summary>
        /// JWT signing key
        /// </summary>
        public string JwtKey { get; set; }
        
        /// <summary>
        /// reCAPTCHA site key
        /// </summary>
        public string RecaptchaSiteKey { get; set; }
        
        /// <summary>
        /// reCAPTCHA secret key
        /// </summary>
        public string RecaptchaSecretKey { get; set; }
        
        /// <summary>
        /// URL of the IP reputation service
        /// </summary>
        public string IpReputationServiceUrl { get; set; }
        
        /// <summary>
        /// Threshold for IP reputation score (higher means more suspicious)
        /// </summary>
        public int IpReputationThreshold { get; set; } = 75;
        
        /// <summary>
        /// Maximum number of failed login attempts before throttling
        /// </summary>
        public int MaxFailedLoginAttempts { get; set; } = 5;
        
        /// <summary>
        /// Maximum number of failed login attempts from an IP before throttling
        /// </summary>
        public int MaxFailedAttemptsPerIp { get; set; } = 10;
        
        /// <summary>
        /// Maximum number of failed login attempts from an IP before blocking
        /// </summary>
        public int MaxFailedAttemptsBeforeIpBlock { get; set; } = 20;
        
        /// <summary>
        /// Duration in minutes for which an IP is blocked after too many failed attempts
        /// </summary>
        public int IpBlockDurationMinutes { get; set; } = 60;
        
        /// <summary>
        /// Duration in minutes after which failed login attempt counters are reset
        /// </summary>
        public int LoginThrottlingResetMinutes { get; set; } = 15;
        
        /// <summary>
        /// Number of days to store private keys in the cache
        /// </summary>
        public int PrivateKeyStorageDays { get; set; } = 1;
        
        /// <summary>
        /// Whether to enable HTTPS redirection
        /// </summary>
        public bool EnableHttpsRedirection { get; set; } = true;
        
        /// <summary>
        /// Whether to enable HSTS (HTTP Strict Transport Security)
        /// </summary>
        public bool EnableHsts { get; set; } = true;
        
        /// <summary>
        /// Whether to enable XSS protection
        /// </summary>
        public bool EnableXssProtection { get; set; } = true;
        
        /// <summary>
        /// Whether to enable CSRF protection
        /// </summary>
        public bool EnableCsrfProtection { get; set; } = true;
        
        /// <summary>
        /// Whether to enable Content Security Policy
        /// </summary>
        public bool EnableContentSecurityPolicy { get; set; } = true;
        
        /// <summary>
        /// Content Security Policy header value
        /// </summary>
        public string ContentSecurityPolicy { get; set; } = "default-src 'self'; script-src 'self' https://www.google.com https://www.gstatic.com; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self';";
    }
}
