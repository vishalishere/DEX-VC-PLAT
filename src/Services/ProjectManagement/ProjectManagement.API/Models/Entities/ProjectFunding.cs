// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.Entities;

public class ProjectFunding
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid InvestorId { get; set; }

    [MaxLength(100)]
    public string InvestorName { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    public FundingType FundingType { get; set; } = FundingType.Investment;

    public FundingStatus Status { get; set; } = FundingStatus.Pending;

    public DateTime FundedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    // Blockchain integration
    [MaxLength(200)]
    public string? TransactionHash { get; set; }

    [MaxLength(200)]
    public string? WalletAddress { get; set; }

    public bool IsVerifiedOnChain { get; set; } = false;

    // Tranche-based funding
    public Guid? MilestoneId { get; set; }

    public ProjectMilestone? Milestone { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Token staking for voting rights
    public decimal StakedTokens { get; set; } = 0;

    public decimal VotingPowerGranted { get; set; } = 0;

    // GDPR compliance for financial data
    public DateTime DataRetentionDate { get; set; } = DateTime.UtcNow.AddYears(7);

    // Navigation property
    public Project Project { get; set; } = null!;
}

public enum FundingType
{
    Investment = 0,
    Grant = 1,
    Loan = 2,
    TokenPurchase = 3
}

public enum FundingStatus
{
    Pending = 0,
    Confirmed = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5,
    Refunded = 6
}
