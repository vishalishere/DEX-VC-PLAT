// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Identity;
using Shared.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace Shared.Security.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(200)]
    public string? WalletAddress { get; set; }

    public bool IsWalletVerified { get; set; } = false;

    public DateTime? WalletVerifiedAt { get; set; }

    public UserRole Role { get; set; } = UserRole.Founder;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // GDPR and EU AI Act Compliance
    public DateTime? DataRetentionDate { get; set; }

    public bool IsDataProcessingConsented { get; set; } = false;

    public DateTime? ConsentGivenAt { get; set; }

    public DateTime? ConsentWithdrawnAt { get; set; }

    // Profile information for DecVCPlat
    [MaxLength(100)]
    public string? LinkedInProfile { get; set; }

    [MaxLength(100)]
    public string? GitHubProfile { get; set; }

    [MaxLength(100)]
    public string? TwitterProfile { get; set; }

    // Investment and project preferences
    public decimal? MinInvestmentAmount { get; set; }

    public decimal? MaxInvestmentAmount { get; set; }

    [MaxLength(1000)]
    public string? PreferredCategories { get; set; }

    // Risk assessment for founders
    public int? CreditScore { get; set; }

    public bool HasPreviousExperience { get; set; } = false;

    [MaxLength(2000)]
    public string? PreviousExperience { get; set; }

    // Two-factor authentication
    public bool TwoFactorEnabled { get; set; } = false;

    // Account verification
    public bool IsKYCVerified { get; set; } = false;

    public DateTime? KYCVerifiedAt { get; set; }

    [MaxLength(500)]
    public string? KYCDocuments { get; set; }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    public string GetDisplayName() => !string.IsNullOrEmpty(GetFullName()) ? GetFullName() : Email ?? UserName ?? "Unknown User";
}
