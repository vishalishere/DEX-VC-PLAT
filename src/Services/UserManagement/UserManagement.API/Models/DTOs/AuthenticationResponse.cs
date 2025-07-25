// © 2024 DecVCPlat. All rights reserved.

using Shared.Common.Enums;

namespace UserManagement.API.Models.DTOs;

public class AuthenticationResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? TokenExpiration { get; set; }
    public UserResponse? User { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? CompanyName { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public string? WalletAddress { get; set; }
    public bool IsWalletVerified { get; set; }
    public bool IsKYCVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LinkedInProfile { get; set; }
    public string? GitHubProfile { get; set; }
    public string? TwitterProfile { get; set; }
    public decimal? MinInvestmentAmount { get; set; }
    public decimal? MaxInvestmentAmount { get; set; }
    public string? PreferredCategories { get; set; }
    public bool HasPreviousExperience { get; set; }
    public string GetFullName() => $"{FirstName} {LastName}".Trim();
}
