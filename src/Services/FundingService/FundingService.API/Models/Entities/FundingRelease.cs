// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace FundingService.API.Models.Entities;

public class FundingRelease
{
    public Guid Id { get; set; }

    [Required]
    public Guid TrancheId { get; set; }

    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    public ReleaseStatus Status { get; set; } = ReleaseStatus.Pending;

    public DateTime InitiatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }

    // Payment method
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Blockchain;

    // Blockchain details
    [MaxLength(200)]
    public string? FromWalletAddress { get; set; }

    [MaxLength(200)]
    public string? ToWalletAddress { get; set; }

    [MaxLength(200)]
    public string? TransactionHash { get; set; }

    public int? BlockNumber { get; set; }

    public decimal? GasFee { get; set; }

    // Traditional payment details
    [MaxLength(200)]
    public string? BankTransactionId { get; set; }

    [MaxLength(100)]
    public string? BankReference { get; set; }

    // Processing details
    public Guid ProcessedByUserId { get; set; }

    [MaxLength(100)]
    public string ProcessedByUserName { get; set; } = string.Empty;

    public decimal ProcessingFeeCharged { get; set; } = 0;

    [MaxLength(1000)]
    public string? ProcessingNotes { get; set; }

    // Verification and compliance
    public bool IsVerified { get; set; } = false;

    public DateTime? VerifiedAt { get; set; }

    public Guid? VerifiedByUserId { get; set; }

    [MaxLength(100)]
    public string? VerifiedByUserName { get; set; }

    // Risk and compliance
    public bool IsHighRisk { get; set; } = false;

    public bool RequiresManualReview { get; set; } = false;

    [MaxLength(1000)]
    public string? RiskAssessmentNotes { get; set; }

    // GDPR and regulatory compliance
    public DateTime DataRetentionDate { get; set; } = DateTime.UtcNow.AddYears(7);

    public bool IsAuditable { get; set; } = true;

    [MaxLength(500)]
    public string? ComplianceNotes { get; set; }

    // Navigation property
    public FundingTranche Tranche { get; set; } = null!;
}

public enum ReleaseStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    UnderReview = 5,
    Disputed = 6
}

public enum PaymentMethod
{
    Blockchain = 0,
    BankTransfer = 1,
    PayPal = 2,
    Stripe = 3,
    Other = 99
}
