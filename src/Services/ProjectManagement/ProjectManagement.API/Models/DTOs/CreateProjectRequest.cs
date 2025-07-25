// © 2024 DecVCPlat. All rights reserved.

using ProjectManagement.API.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.DTOs;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Project title is required")]
    [StringLength(200, ErrorMessage = "Project title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project description is required")]
    [StringLength(5000, ErrorMessage = "Project description cannot exceed 5000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Funding goal is required")]
    [Range(1000, 100000000, ErrorMessage = "Funding goal must be between $1,000 and $100,000,000")]
    public decimal FundingGoal { get; set; }

    [StringLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Invalid image URL format")]
    public string? ImageUrl { get; set; }

    [StringLength(500, ErrorMessage = "Video URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Invalid video URL format")]
    public string? VideoUrl { get; set; }

    [StringLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
    [Url(ErrorMessage = "Invalid website URL format")]
    public string? WebsiteUrl { get; set; }

    [StringLength(200, ErrorMessage = "GitHub URL cannot exceed 200 characters")]
    [Url(ErrorMessage = "Invalid GitHub URL format")]
    public string? GitHubUrl { get; set; }

    [StringLength(200, ErrorMessage = "Whitepaper URL cannot exceed 200 characters")]
    [Url(ErrorMessage = "Invalid whitepaper URL format")]
    public string? WhitepaperUrl { get; set; }

    [Required(ErrorMessage = "Risk level assessment is required")]
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Medium;

    [StringLength(1000, ErrorMessage = "Risk assessment cannot exceed 1000 characters")]
    public string? RiskAssessment { get; set; }

    [Required(ErrorMessage = "Funding deadline is required")]
    [FutureDate(ErrorMessage = "Funding deadline must be in the future")]
    public DateTime FundingDeadline { get; set; }

    // Milestones for tranche-based funding
    [Required(ErrorMessage = "At least one milestone is required")]
    [MinLength(1, ErrorMessage = "At least one milestone is required")]
    public List<CreateMilestoneRequest> Milestones { get; set; } = new();
}

public class CreateMilestoneRequest
{
    [Required(ErrorMessage = "Milestone title is required")]
    [StringLength(200, ErrorMessage = "Milestone title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Milestone description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Funding amount is required")]
    [Range(100, 10000000, ErrorMessage = "Funding amount must be between $100 and $10,000,000")]
    public decimal FundingAmount { get; set; }

    [Required(ErrorMessage = "Due date is required")]
    [FutureDate(ErrorMessage = "Due date must be in the future")]
    public DateTime DueDate { get; set; }
}

// Custom validation attribute for future dates
public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime > DateTime.UtcNow;
        }
        return false;
    }
}
