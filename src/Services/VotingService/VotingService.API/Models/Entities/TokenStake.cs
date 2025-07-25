// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace VotingService.API.Models.Entities;

public class TokenStake
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProposalId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string WalletAddress { get; set; } = string.Empty;

    [Required]
    public decimal TokenAmount { get; set; }

    public StakeStatus Status { get; set; } = StakeStatus.Active;

    public DateTime StakedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UnstakedAt { get; set; }

    // Blockchain verification
    [MaxLength(200)]
    public string? TransactionHash { get; set; }

    [MaxLength(200)]
    public string? UnstakeTransactionHash { get; set; }

    public bool IsVerifiedOnChain { get; set; } = false;

    public int BlockNumber { get; set; } = 0;

    // Lock period for governance
    public DateTime LockedUntil { get; set; }

    public bool IsLocked => DateTime.UtcNow < LockedUntil;

    // Rewards and penalties
    public decimal RewardEarned { get; set; } = 0;

    public decimal PenaltyApplied { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation property
    public VotingProposal Proposal { get; set; } = null!;
}

public enum StakeStatus
{
    Pending = 0,
    Active = 1,
    Unstaked = 2,
    Slashed = 3,
    Expired = 4
}
