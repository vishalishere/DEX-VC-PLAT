// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace VotingService.API.Models.Entities;

public class Vote
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProposalId { get; set; }

    [Required]
    public Guid VoterId { get; set; }

    [MaxLength(100)]
    public string VoterName { get; set; } = string.Empty;

    [Required]
    public VoteChoice Choice { get; set; }

    [Required]
    public decimal VotingPower { get; set; }

    public DateTime CastAt { get; set; } = DateTime.UtcNow;

    [MaxLength(1000)]
    public string? Comments { get; set; }

    // Blockchain verification
    [MaxLength(200)]
    public string? TransactionHash { get; set; }

    public bool IsVerifiedOnChain { get; set; } = false;

    public int BlockNumber { get; set; } = 0;

    // Vote delegation
    public bool IsDelegated { get; set; } = false;

    public Guid? DelegatedFromUserId { get; set; }

    [MaxLength(100)]
    public string? DelegatedFromUserName { get; set; }

    // Privacy and security
    public bool IsAnonymous { get; set; } = false;

    [MaxLength(200)]
    public string? EncryptedVoteHash { get; set; }

    // Vote weight calculation
    public decimal BaseVotingPower { get; set; }

    public decimal StakeMultiplier { get; set; } = 1.0m;

    public decimal ReputationMultiplier { get; set; } = 1.0m;

    // Navigation property
    public VotingProposal Proposal { get; set; } = null!;
}

public enum VoteChoice
{
    Against = 0,
    Support = 1,
    Abstain = 2
}
