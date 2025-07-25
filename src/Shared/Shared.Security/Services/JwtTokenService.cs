// © 2024 DecVCPlat. All rights reserved.

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Security.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Shared.Security.Services;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["JWT:SecretKey"] ?? "DecVCPlat-Super-Secret-Key-For-JWT-Tokens-2024-Must-Be-At-Least-256-Bits-Long!";
        _issuer = _configuration["JWT:Issuer"] ?? "DecVCPlat";
        _audience = _configuration["JWT:Audience"] ?? "DecVCPlat-Users";
        _expirationMinutes = int.Parse(_configuration["JWT:ExpirationMinutes"] ?? "1440"); // 24 hours default
    }

    public string GenerateToken(ApplicationUser user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("user_id", user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("role", user.Role.ToString()),
            new("full_name", user.GetFullName()),
            new("wallet_address", user.WalletAddress ?? string.Empty),
            new("wallet_verified", user.IsWalletVerified.ToString()),
            new("kyc_verified", user.IsKYCVerified.ToString()),
            new("account_status", user.Status.ToString()),
            new("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add role-specific claims for DecVCPlat
        switch (user.Role.ToString())
        {
            case "Founder":
                claims.Add(new Claim("can_submit_projects", "true"));
                claims.Add(new Claim("can_request_funding", "true"));
                break;
            case "Investor":
                claims.Add(new Claim("can_vote", "true"));
                claims.Add(new Claim("can_stake_tokens", "true"));
                claims.Add(new Claim("min_investment", user.MinInvestmentAmount?.ToString() ?? "0"));
                claims.Add(new Claim("max_investment", user.MaxInvestmentAmount?.ToString() ?? "0"));
                break;
            case "Luminary":
                claims.Add(new Claim("can_review_projects", "true"));
                claims.Add(new Claim("can_approve_funding", "true"));
                claims.Add(new Claim("can_manage_voting", "true"));
                break;
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
