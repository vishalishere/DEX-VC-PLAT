// © 2024 DecVCPlat. All rights reserved.

using FundingService.API.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace FundingService.API.Models.DTOs;

public class CreateTrancheRequest
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Milestone ID is required")]
    public Guid MilestoneId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(100, 10000000, ErrorMessage = "Amount must be between $100 and $10,000,000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Tranche number is required")]
    [Range(1, 100, ErrorMessage = "Tranche number must be between 1 and 100")]
    public int TrancheNumber { get; set; }

    [Required(ErrorMessage = "Scheduled release date is required")]
    public DateTime ScheduledReleaseDate { get; set; }

    [Range(1, 100, ErrorMessage = "Minimum voting threshold must be between 1% and 100%")]
    public decimal MinVotingThreshold { get; set; } = 51.0m;

    public bool RequiresBoardApproval { get; set; } = true;

    public bool RequiresMilestoneEvidence { get; set; } = true;

    [StringLength(200, ErrorMessage = "Recipient wallet address cannot exceed 200 characters")]
    public string? RecipientWalletAddress { get; set; }
}

public class ReleaseFundsRequest
{
    [Required(ErrorMessage = "Tranche ID is required")]
    public Guid TrancheId { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(1, 10000000, ErrorMessage = "Amount must be between $1 and $10,000,000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Payment method is required")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Blockchain;

    [Required(ErrorMessage = "Recipient wallet address is required")]
    [StringLength(200, ErrorMessage = "Recipient wallet address cannot exceed 200 characters")]
    public string RecipientWalletAddress { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Processing notes cannot exceed 1000 characters")]
    public string? ProcessingNotes { get; set; }

    public bool RequiresManualReview { get; set; } = false;
}

public class ApproveFundsRequest
{
    [Required(ErrorMessage = "Tranche ID is required")]
    public Guid TrancheId { get; set; }

    [StringLength(500, ErrorMessage = "Milestone evidence URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Invalid milestone evidence URL format")]
    public string? MilestoneEvidenceUrl { get; set; }

    [Required(ErrorMessage = "Approval decision is required")]
    public bool IsApproved { get; set; }

    [StringLength(1000, ErrorMessage = "Approval notes cannot exceed 1000 characters")]
    public string? ApprovalNotes { get; set; }
}

public class TrancheResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TrancheNumber { get; set; }
    public TrancheStatus Status { get; set; }
    public DateTime ScheduledReleaseDate { get; set; }
    public DateTime? ActualReleaseDate { get; set; }
    public bool IsMilestoneCompleted { get; set; }
    public bool IsLuminaryApproved { get; set; }
    public Guid? ApprovedByLuminaryId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? SmartContractAddress { get; set; }
    public string? ReleaseTransactionHash { get; set; }
    public bool IsReleasedOnChain { get; set; }
    public string? EscrowWalletAddress { get; set; }
    public bool IsInEscrow { get; set; }
    public decimal MinVotingThreshold { get; set; }
    public bool RequiresBoardApproval { get; set; }
    public bool RequiresMilestoneEvidence { get; set; }
    public string? MilestoneEvidenceUrl { get; set; }
    public decimal ProcessingFee { get; set; }
    public decimal ActualAmountReleased { get; set; }
    public string? RecipientWalletAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ReleaseNotes { get; set; }
    public List<ReleaseResponse> Releases { get; set; } = new();
}

public class ReleaseResponse
{
    public Guid Id { get; set; }
    public Guid TrancheId { get; set; }
    public Guid ProjectId { get; set; }
    public decimal Amount { get; set; }
    public ReleaseStatus Status { get; set; }
    public DateTime InitiatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string? FromWalletAddress { get; set; }
    public string? ToWalletAddress { get; set; }
    public string? TransactionHash { get; set; }
    public int? BlockNumber { get; set; }
    public decimal? GasFee { get; set; }
    public string? BankTransactionId { get; set; }
    public string? BankReference { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public string ProcessedByUserName { get; set; } = string.Empty;
    public decimal ProcessingFeeCharged { get; set; }
    public string? ProcessingNotes { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsHighRisk { get; set; }
    public bool RequiresManualReview { get; set; }
}
