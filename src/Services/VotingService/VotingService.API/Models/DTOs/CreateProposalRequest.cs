// © 2024 DecVCPlat. All rights reserved.

using VotingService.API.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace VotingService.API.Models.DTOs;

public class CreateProposalRequest
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Proposal title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Proposal type is required")]
    public ProposalType ProposalType { get; set; }

    [Required(ErrorMessage = "End date is required")]
    public DateTime EndDate { get; set; }

    [Range(1, 1000000, ErrorMessage = "Minimum tokens required must be between 1 and 1,000,000")]
    public decimal MinTokensRequired { get; set; } = 100;

    [Range(1, 100, ErrorMessage = "Quorum percentage must be between 1 and 100")]
    public decimal QuorumRequired { get; set; } = 51;

    public bool RequiresLuminaryApproval { get; set; } = true;
}

public class StakeTokensRequest
{
    [Required(ErrorMessage = "Proposal ID is required")]
    public Guid ProposalId { get; set; }

    [Required(ErrorMessage = "Token amount is required")]
    [Range(1, 1000000, ErrorMessage = "Token amount must be between 1 and 1,000,000")]
    public decimal TokenAmount { get; set; }

    [Required(ErrorMessage = "Wallet address is required")]
    [StringLength(200, ErrorMessage = "Wallet address cannot exceed 200 characters")]
    public string WalletAddress { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

public class CastVoteRequest
{
    [Required(ErrorMessage = "Proposal ID is required")]
    public Guid ProposalId { get; set; }

    [Required(ErrorMessage = "Vote choice is required")]
    public VoteChoice Choice { get; set; }

    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; }

    public bool IsAnonymous { get; set; } = false;
}

public class DelegateVoteRequest
{
    [Required(ErrorMessage = "Proposal ID is required")]
    public Guid ProposalId { get; set; }

    [Required(ErrorMessage = "Delegate user ID is required")]
    public Guid DelegateToUserId { get; set; }

    [StringLength(500, ErrorMessage = "Delegation reason cannot exceed 500 characters")]
    public string? DelegationReason { get; set; }
}
