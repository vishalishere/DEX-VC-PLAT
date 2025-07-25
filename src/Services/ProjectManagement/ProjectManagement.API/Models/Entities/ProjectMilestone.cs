// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.Entities;

public class ProjectMilestone
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal FundingAmount { get; set; }

    public DateTime DueDate { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedByUserId { get; set; }

    [MaxLength(1000)]
    public string? CompletionNotes { get; set; }

    [MaxLength(500)]
    public string? ProofDocumentUrl { get; set; }

    // Smart contract milestone verification
    [MaxLength(200)]
    public string? BlockchainTransactionHash { get; set; }

    public bool IsFundingReleased { get; set; } = false;

    // Navigation property
    public Project Project { get; set; } = null!;
}

public enum MilestoneStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Approved = 3,
    Rejected = 4,
    FundingReleased = 5
}
