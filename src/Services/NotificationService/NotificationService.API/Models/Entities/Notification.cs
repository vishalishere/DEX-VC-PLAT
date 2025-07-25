// © 2024 DecVCPlat. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Models.Entities;

public class Notification
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

    // Related entities
    public Guid? ProjectId { get; set; }

    public Guid? ProposalId { get; set; }

    public Guid? TrancheId { get; set; }

    public Guid? MilestoneId { get; set; }

    // Delivery channels
    public bool SendEmail { get; set; } = true;

    public bool SendPush { get; set; } = true;

    public bool SendInApp { get; set; } = true;

    public bool SendSMS { get; set; } = false;

    // Delivery tracking
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    // Email details
    [MaxLength(200)]
    public string? EmailSubject { get; set; }

    [MaxLength(5000)]
    public string? EmailBody { get; set; }

    public bool IsEmailSent { get; set; } = false;

    public DateTime? EmailSentAt { get; set; }

    [MaxLength(500)]
    public string? EmailError { get; set; }

    // Push notification details
    [MaxLength(1000)]
    public string? PushPayload { get; set; }

    public bool IsPushSent { get; set; } = false;

    public DateTime? PushSentAt { get; set; }

    [MaxLength(500)]
    public string? PushError { get; set; }

    // Action buttons and deep links
    [MaxLength(200)]
    public string? ActionUrl { get; set; }

    [MaxLength(100)]
    public string? ActionText { get; set; }

    // Metadata for customization
    [MaxLength(2000)]
    public string? MetadataJson { get; set; }

    // GDPR compliance
    public DateTime DataRetentionDate { get; set; } = DateTime.UtcNow.AddMonths(6);

    public bool IsPersonalData { get; set; } = true;

    // Analytics
    public bool IsTracked { get; set; } = true;

    public int OpenCount { get; set; } = 0;

    public int ClickCount { get; set; } = 0;

    // Batch processing
    public Guid? BatchId { get; set; }

    public bool IsPartOfBatch { get; set; } = false;
}

public enum NotificationType
{
    ProjectSubmitted = 0,
    ProjectApproved = 1,
    ProjectRejected = 2,
    FundingGoalReached = 3,
    MilestoneCompleted = 4,
    MilestoneApproved = 5,
    FundsReleased = 6,
    VotingStarted = 7,
    VotingEnded = 8,
    NewInvestment = 9,
    NewComment = 10,
    SystemAnnouncement = 11,
    SecurityAlert = 12,
    WalletConnected = 13,
    KYCApproved = 14,
    AccountVerified = 15,
    Other = 99
}

public enum NotificationPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Read = 3,
    Failed = 4,
    Expired = 5,
    Cancelled = 6
}
