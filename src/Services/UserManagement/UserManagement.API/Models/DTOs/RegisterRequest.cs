// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Shared.Common.Enums;

namespace UserManagement.API.Models.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare("Password", ErrorMessage = "Password and confirmation do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "Company name cannot exceed 200 characters")]
    public string? CompanyName { get; set; }

    [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string? Bio { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public UserRole Role { get; set; } = UserRole.Founder;

    [MaxLength(200, ErrorMessage = "Wallet address cannot exceed 200 characters")]
    public string? WalletAddress { get; set; }

    // GDPR and EU AI Act Compliance
    [Required(ErrorMessage = "Data processing consent is required")]
    public bool IsDataProcessingConsented { get; set; } = false;

    // Social profiles
    [MaxLength(100, ErrorMessage = "LinkedIn profile cannot exceed 100 characters")]
    public string? LinkedInProfile { get; set; }

    [MaxLength(100, ErrorMessage = "GitHub profile cannot exceed 100 characters")]
    public string? GitHubProfile { get; set; }

    [MaxLength(100, ErrorMessage = "Twitter profile cannot exceed 100 characters")]
    public string? TwitterProfile { get; set; }

    // Investment preferences for investors
    public decimal? MinInvestmentAmount { get; set; }

    public decimal? MaxInvestmentAmount { get; set; }

    [MaxLength(1000, ErrorMessage = "Preferred categories cannot exceed 1000 characters")]
    public string? PreferredCategories { get; set; }

    // Founder experience
    public bool HasPreviousExperience { get; set; } = false;

    [MaxLength(2000, ErrorMessage = "Previous experience cannot exceed 2000 characters")]
    public string? PreviousExperience { get; set; }
}
