// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace VotingService.API.Models.Entities;

public class VotingProposal
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public ProposalType ProposalType { get; set; }

    public VotingStatus Status { get; set; } = VotingStatus.Active;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    public DateTime EndDate { get; set; }

    public decimal MinTokensRequired { get; set; } = 100;

    public decimal QuorumRequired { get; set; } = 51; // Percentage

    // Voting results
    public decimal TotalTokensStaked { get; set; } = 0;

    public int TotalVoters { get; set; } = 0;

    public decimal SupportTokens { get; set; } = 0;

    public decimal AgainstTokens { get; set; } = 0;

    public decimal AbstainTokens { get; set; } = 0;

    // Blockchain integration
    [MaxLength(200)]
    public string? SmartContractAddress { get; set; }

    [MaxLength(200)]
    public string? DeploymentTransactionHash { get; set; }

    public bool IsOnChain { get; set; } = false;

    // Governance parameters
    public bool RequiresLuminaryApproval { get; set; } = true;

    public Guid? ApprovedByLuminaryId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExecutedAt { get; set; }

    [MaxLength(1000)]
    public string? ExecutionNotes { get; set; }

    // Navigation properties
    public ICollection<TokenStake> TokenStakes { get; set; } = new List<TokenStake>();
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();

    // Calculated properties
    public decimal SupportPercentage => TotalTokensStaked > 0 ? (SupportTokens / TotalTokensStaked) * 100 : 0;
    public decimal AgainstPercentage => TotalTokensStaked > 0 ? (AgainstTokens / TotalTokensStaked) * 100 : 0;
    public bool HasMetQuorum => SupportPercentage >= QuorumRequired;
    public bool IsExpired => DateTime.UtcNow > EndDate;
    public bool CanExecute => HasMetQuorum && IsExpired && Status == VotingStatus.Active;
}

public enum ProposalType
{
    ProjectApproval = 0,
    FundingRelease = 1,
    MilestoneApproval = 2,
    GovernanceChange = 3,
    EmergencyAction = 4
}

public enum VotingStatus
{
    Draft = 0,
    Active = 1,
    Passed = 2,
    Failed = 3,
    Executed = 4,
    Cancelled = 5
}
