// © 2024 DecVCPlat. All rights reserved.

using FluentValidation;
using UserManagement.API.Models.DTOs;
using Shared.Common.Enums;

namespace UserManagement.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required for DecVCPlat registration")
            .EmailAddress().WithMessage("Please provide a valid email address")
            .MaximumLength(254).WithMessage("Email address is too long");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Password and confirmation password must match");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z\s\-'\.]+$").WithMessage("Last name contains invalid characters");

        RuleFor(x => x.CompanyName)
            .MaximumLength(200).WithMessage("Company name cannot exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.CompanyName));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role specified for DecVCPlat platform");

        // Wallet address validation for blockchain integration
        RuleFor(x => x.WalletAddress)
            .Matches(@"^0x[a-fA-F0-9]{40}$")
            .WithMessage("Wallet address must be a valid Ethereum address (0x followed by 40 hex characters)")
            .When(x => !string.IsNullOrEmpty(x.WalletAddress));

        // GDPR compliance validation
        RuleFor(x => x.IsDataProcessingConsented)
            .Equal(true).WithMessage("You must consent to data processing to register on DecVCPlat platform");

        // Social profile validations
        RuleFor(x => x.LinkedInProfile)
            .Matches(@"^(https?://)?(www\.)?linkedin\.com/in/[\w\-]+/?$")
            .WithMessage("LinkedIn profile must be a valid LinkedIn URL")
            .When(x => !string.IsNullOrEmpty(x.LinkedInProfile));

        RuleFor(x => x.GitHubProfile)
            .Matches(@"^(https?://)?(www\.)?github\.com/[\w\-]+/?$")
            .WithMessage("GitHub profile must be a valid GitHub URL")
            .When(x => !string.IsNullOrEmpty(x.GitHubProfile));

        RuleFor(x => x.TwitterProfile)
            .Matches(@"^(https?://)?(www\.)?twitter\.com/[\w\-]+/?$")
            .WithMessage("Twitter profile must be a valid Twitter URL")
            .When(x => !string.IsNullOrEmpty(x.TwitterProfile));

        // Investment amount validations for investors
        RuleFor(x => x.MinInvestmentAmount)
            .GreaterThan(0).WithMessage("Minimum investment amount must be greater than 0")
            .LessThanOrEqualTo(x => x.MaxInvestmentAmount).WithMessage("Minimum investment cannot be greater than maximum")
            .When(x => x.Role == UserRole.Investor && x.MinInvestmentAmount.HasValue);

        RuleFor(x => x.MaxInvestmentAmount)
            .GreaterThan(0).WithMessage("Maximum investment amount must be greater than 0")
            .GreaterThanOrEqualTo(x => x.MinInvestmentAmount).WithMessage("Maximum investment cannot be less than minimum")
            .When(x => x.Role == UserRole.Investor && x.MaxInvestmentAmount.HasValue);

        // Experience validation for founders
        RuleFor(x => x.PreviousExperience)
            .NotEmpty().WithMessage("Please describe your previous experience")
            .MaximumLength(2000).WithMessage("Previous experience description cannot exceed 2000 characters")
            .When(x => x.Role == UserRole.Founder && x.HasPreviousExperience);
    }
}
