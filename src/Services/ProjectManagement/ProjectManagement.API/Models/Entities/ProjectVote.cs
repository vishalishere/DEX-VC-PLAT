// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.API.Models.Entities;

public class ProjectVote
{
    public Guid Id { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid VoterId { get; set; }

    [MaxLength(100)]
    public string VoterName { get; set; } = string.Empty;

    public VoteType VoteType { get; set; }

    [Required]
    public decimal StakedTokens { get; set; }

    public decimal VotingPower { get; set; }

    [MaxLength(1000)]
    public string? Comments { get; set; }

    public DateTime VotedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(200)]
    public string? BlockchainTransactionHash { get; set; }

    public bool IsVerifiedOnChain { get; set; } = false;

    // Navigation property
    public Project Project { get; set; } = null!;
}

public enum VoteType
{
    Support = 1,
    Against = 0,
    Abstain = -1
}
