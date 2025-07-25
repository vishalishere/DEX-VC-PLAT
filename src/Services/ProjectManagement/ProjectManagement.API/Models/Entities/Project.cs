// © 2024 DecVCPlat. All rights reserved.

using Shared.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.Entities;

public class Project
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public Guid FounderId { get; set; }

    [MaxLength(100)]
    public string FounderName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    public decimal FundingGoal { get; set; }

    public decimal CurrentFunding { get; set; } = 0;

    public int VotingPower { get; set; } = 0;

    public int TotalVotes { get; set; } = 0;

    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    [MaxLength(200)]
    public string? WebsiteUrl { get; set; }

    [MaxLength(200)]
    public string? GitHubUrl { get; set; }

    [MaxLength(200)]
    public string? WhitepaperUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime? FundingDeadline { get; set; }

    // Risk assessment
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Medium;

    [MaxLength(1000)]
    public string? RiskAssessment { get; set; }

    // Smart contract integration
    [MaxLength(200)]
    public string? SmartContractAddress { get; set; }

    public bool IsSmartContractDeployed { get; set; } = false;

    // Collections
    public ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    public ICollection<ProjectVote> Votes { get; set; } = new List<ProjectVote>();
    public ICollection<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
    public ICollection<ProjectComment> Comments { get; set; } = new List<ProjectComment>();
    public ICollection<ProjectFunding> FundingRounds { get; set; } = new List<ProjectFunding>();
}

public enum ProjectStatus
{
    Draft = 0,
    Submitted = 1,
    UnderReview = 2,
    Approved = 3,
    Rejected = 4,
    Funded = 5,
    InProgress = 6,
    Completed = 7,
    Cancelled = 8
}

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    VeryHigh = 3
}
