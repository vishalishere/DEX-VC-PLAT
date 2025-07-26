# DecVCPlat Security System

This document describes the security features implemented in the DecVCPlat platform.

## Overview

The DecVCPlat security system provides comprehensive protection for user data, prevents common web vulnerabilities, and ensures secure blockchain transactions. It implements industry best practices for authentication, authorization, data protection, and secure communication.

## Architecture

The security system consists of several components:

1. **Security Service**: Core service providing security-related functionalities
2. **Authentication & Authorization**: JWT-based authentication and role-based authorization
3. **Data Protection**: Encryption for sensitive data
4. **Input Validation & Sanitization**: Protection against injection attacks
5. **Rate Limiting & Throttling**: Prevention of brute force attacks
6. **Blockchain Security**: Secure handling of wallet addresses and private keys
7. **Security Headers**: Protection against common web vulnerabilities

## Features

### Password Security

- Configurable password complexity requirements
- PBKDF2 password hashing with salt
- Protection against common passwords
- Secure password verification

### Authentication & Authorization

- JWT-based authentication
- Role-based access control
- Token validation and revocation
- Secure token generation

### Data Protection

- AES encryption for sensitive data
- Purpose-based key derivation
- Secure storage of private keys
- Encrypted communication via HTTPS

### Input Validation & Sanitization

- Input sanitization to prevent XSS attacks
- Content Security Policy implementation
- Protection against injection attacks

### Rate Limiting & Throttling

- Login attempt throttling
- IP-based rate limiting
- Temporary IP blocking for suspicious activity
- Protection against brute force attacks

### CAPTCHA Integration

- reCAPTCHA integration for form submissions
- Protection against automated attacks

### IP Reputation Checking

- Integration with IP reputation services
- Blocking of known malicious IPs
- Monitoring of suspicious activity

### Blockchain Security

- Validation of blockchain addresses
- Secure storage of private keys
- Encrypted communication with blockchain nodes

### Security Headers

- Content Security Policy (CSP)
- X-Content-Type-Options
- X-Frame-Options
- X-XSS-Protection
- Referrer-Policy
- Permissions-Policy
- HTTP Strict Transport Security (HSTS)

## Configuration

Security settings are configured in the application's configuration files:

```json
{
  "Security": {
    "MinPasswordLength": 8,
    "RequireUppercase": true,
    "RequireLowercase": true,
    "RequireDigit": true,
    "RequireSpecialCharacter": true,
    "CheckCommonPasswords": true,
    "EncryptionKey": "your-secure-encryption-key",
    "JwtIssuer": "DecVCPlat",
    "JwtAudience": "DecVCPlatUsers",
    "JwtKey": "your-secure-jwt-signing-key",
    "RecaptchaSiteKey": "your-recaptcha-site-key",
    "RecaptchaSecretKey": "your-recaptcha-secret-key",
    "IpReputationServiceUrl": "https://ip-reputation-service.example.com/check",
    "IpReputationThreshold": 75,
    "MaxFailedLoginAttempts": 5,
    "MaxFailedAttemptsPerIp": 10,
    "MaxFailedAttemptsBeforeIpBlock": 20,
    "IpBlockDurationMinutes": 60,
    "LoginThrottlingResetMinutes": 15,
    "PrivateKeyStorageDays": 1,
    "EnableHttpsRedirection": true,
    "EnableHsts": true,
    "EnableXssProtection": true,
    "EnableCsrfProtection": true,
    "EnableContentSecurityPolicy": true,
    "ContentSecurityPolicy": "default-src 'self'; script-src 'self' https://www.google.com https://www.gstatic.com; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self';"
  }
}
```

## Usage Examples

### Registering Security Services

```csharp
// In Startup.cs or Program.cs
services.AddSecurityServices(Configuration);

// In Configure method
app.UseSecurityMiddleware(Configuration);
```

### Password Validation

```csharp
// Validate password complexity
var (isValid, errorMessages) = await _securityService.ValidatePasswordAsync(password);
if (!isValid)
{
    // Handle invalid password
    foreach (var error in errorMessages)
    {
        ModelState.AddModelError("Password", error);
    }
    return View(model);
}

// Hash password for storage
var hashedPassword = await _securityService.HashPasswordAsync(password);

// Verify password during login
var isPasswordValid = await _securityService.VerifyPasswordAsync(password, user.HashedPassword);
```

### JWT Token Generation and Validation

```csharp
// Generate JWT token
var token = await _securityService.GenerateJwtTokenAsync(
    user.Id,
    user.Email,
    user.Roles);

// Validate JWT token
var (isValid, principal) = await _securityService.ValidateTokenAsync(token);
if (isValid)
{
    // Token is valid, use principal for authorization
}
```

### Encrypting Sensitive Data

```csharp
// Encrypt sensitive data
var encryptedData = await _securityService.EncryptAsync(sensitiveData);

// Decrypt sensitive data
var decryptedData = await _securityService.DecryptAsync(encryptedData);
```

### Input Sanitization

```csharp
// Sanitize user input
var sanitizedInput = _securityService.SanitizeInput(userInput);
```

### Rate Limiting

```csharp
// Check if login attempts should be throttled
var shouldThrottle = await _securityService.ShouldThrottleLoginAttemptsAsync(username, ipAddress);
if (shouldThrottle)
{
    // Return error response or delay
    return StatusCode(429, "Too many login attempts. Please try again later.");
}

// Record failed login attempt
await _securityService.RecordFailedLoginAttemptAsync(username, ipAddress);
```

### CAPTCHA Verification

```csharp
// Verify CAPTCHA response
var isCaptchaValid = await _securityService.VerifyCaptchaAsync(captchaResponse, ipAddress);
if (!isCaptchaValid)
{
    // Handle invalid CAPTCHA
    ModelState.AddModelError("Captcha", "CAPTCHA verification failed.");
    return View(model);
}
```

### Blockchain Address Validation

```csharp
// Validate blockchain address
var isValidAddress = _securityService.IsValidBlockchainAddress(address);
if (!isValidAddress)
{
    // Handle invalid address
    ModelState.AddModelError("Address", "Invalid blockchain address.");
    return View(model);
}
```

### Secure Private Key Storage

```csharp
// Store private key securely
await _securityService.StorePrivateKeyAsync(userId, privateKey);

// Retrieve private key when needed
var privateKey = await _securityService.RetrievePrivateKeyAsync(userId);
```

## Security Best Practices

1. **Environment Variables**: Store sensitive configuration values (keys, secrets) in environment variables or a secure vault, not in configuration files.

2. **Regular Updates**: Keep all dependencies and packages up to date to address security vulnerabilities.

3. **Security Scanning**: Regularly scan the codebase for security vulnerabilities using tools like OWASP ZAP or Snyk.

4. **Audit Logging**: Implement comprehensive audit logging for security-related events.

5. **Least Privilege**: Follow the principle of least privilege for all operations.

6. **Defense in Depth**: Implement multiple layers of security controls.

7. **Regular Backups**: Maintain regular backups of all data.

8. **Incident Response Plan**: Have a plan for responding to security incidents.

## Dependencies

- **Microsoft.AspNetCore.Cryptography.KeyDerivation**: For password hashing
- **System.IdentityModel.Tokens.Jwt**: For JWT token handling
- **Nethereum.Util**: For blockchain address validation
- **Microsoft.Extensions.Caching.Distributed**: For rate limiting and token storage
