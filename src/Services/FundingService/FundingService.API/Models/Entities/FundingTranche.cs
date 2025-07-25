// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace FundingService.API.Models.Entities;

public class FundingTranche
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid MilestoneId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public int TrancheNumber { get; set; }

    public TrancheStatus Status { get; set; } = TrancheStatus.Pending;

    public DateTime ScheduledReleaseDate { get; set; }

    public DateTime? ActualReleaseDate { get; set; }

    // Milestone conditions
    public bool IsMilestoneCompleted { get; set; } = false;

    public bool IsLuminaryApproved { get; set; } = false;

    public Guid? ApprovedByLuminaryId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    // Smart contract integration
    [MaxLength(200)]
    public string? SmartContractAddress { get; set; }

    [MaxLength(200)]
    public string? ReleaseTransactionHash { get; set; }

    public bool IsReleasedOnChain { get; set; } = false;

    // Escrow details
    [MaxLength(200)]
    public string? EscrowWalletAddress { get; set; }

    public bool IsInEscrow { get; set; } = false;

    public DateTime? EscrowCreatedAt { get; set; }

    // Release conditions
    public decimal MinVotingThreshold { get; set; } = 51.0m; // Percentage

    public bool RequiresBoardApproval { get; set; } = true;

    public bool RequiresMilestoneEvidence { get; set; } = true;

    [MaxLength(500)]
    public string? MilestoneEvidenceUrl { get; set; }

    // Financial tracking
    public decimal ProcessingFee { get; set; } = 0;

    public decimal ActualAmountReleased { get; set; } = 0;

    [MaxLength(200)]
    public string? RecipientWalletAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(1000)]
    public string? ReleaseNotes { get; set; }

    // Navigation properties
    public ICollection<FundingRelease> Releases { get; set; } = new List<FundingRelease>();
}

public enum TrancheStatus
{
    Pending = 0,
    InEscrow = 1,
    AwaitingApproval = 2,
    Approved = 3,
    Released = 4,
    Disputed = 5,
    Cancelled = 6,
    Failed = 7
}
