// © 2024 DecVCPlat. All rights reserved.

using NotificationService.API.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace NotificationService.API.Models.DTOs;

public class CreateNotificationRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification type is required")]
    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

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

    // Email details
    [StringLength(200, ErrorMessage = "Email subject cannot exceed 200 characters")]
    public string? EmailSubject { get; set; }

    [StringLength(5000, ErrorMessage = "Email body cannot exceed 5000 characters")]
    public string? EmailBody { get; set; }

    // Action details
    [StringLength(200, ErrorMessage = "Action URL cannot exceed 200 characters")]
    [Url(ErrorMessage = "Invalid action URL format")]
    public string? ActionUrl { get; set; }

    [StringLength(100, ErrorMessage = "Action text cannot exceed 100 characters")]
    public string? ActionText { get; set; }

    // Expiration
    public DateTime? ExpiresAt { get; set; }

    // Metadata
    [StringLength(2000, ErrorMessage = "Metadata cannot exceed 2000 characters")]
    public string? MetadataJson { get; set; }
}

public class BulkNotificationRequest
{
    [Required(ErrorMessage = "User IDs are required")]
    [MinLength(1, ErrorMessage = "At least one user ID is required")]
    public List<Guid> UserIds { get; set; } = new();

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required")]
    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification type is required")]
    public NotificationType Type { get; set; }

    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    // Related entities
    public Guid? ProjectId { get; set; }
    public Guid? ProposalId { get; set; }

    // Delivery channels
    public bool SendEmail { get; set; } = true;
    public bool SendPush { get; set; } = true;
    public bool SendInApp { get; set; } = true;

    // Scheduling
    public DateTime? ScheduledAt { get; set; }
}

public class NotificationResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public NotificationPriority Priority { get; set; }
    public NotificationStatus Status { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ProposalId { get; set; }
    public Guid? TrancheId { get; set; }
    public Guid? MilestoneId { get; set; }
    public bool SendEmail { get; set; }
    public bool SendPush { get; set; }
    public bool SendInApp { get; set; }
    public bool SendSMS { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? ActionText { get; set; }
    public bool IsEmailSent { get; set; }
    public bool IsPushSent { get; set; }
    public int OpenCount { get; set; }
    public int ClickCount { get; set; }
    public bool IsRead => ReadAt.HasValue;
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
}

public class MarkAsReadRequest
{
    [Required(ErrorMessage = "Notification IDs are required")]
    [MinLength(1, ErrorMessage = "At least one notification ID is required")]
    public List<Guid> NotificationIds { get; set; } = new();
}

public class NotificationStatsResponse
{
    public int TotalNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public int ReadNotifications { get; set; }
    public int PendingNotifications { get; set; }
    public int SentNotifications { get; set; }
    public int FailedNotifications { get; set; }
    public Dictionary<NotificationType, int> NotificationsByType { get; set; } = new();
    public Dictionary<NotificationPriority, int> NotificationsByPriority { get; set; } = new();
}

public class NotificationPreferencesRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    public bool EnableEmailNotifications { get; set; } = true;
    public bool EnablePushNotifications { get; set; } = true;
    public bool EnableInAppNotifications { get; set; } = true;
    public bool EnableSMSNotifications { get; set; } = false;

    // Notification type preferences
    public Dictionary<NotificationType, bool> TypePreferences { get; set; } = new();
    
    // Quiet hours
    public TimeSpan? QuietHoursStart { get; set; }
    public TimeSpan? QuietHoursEnd { get; set; }
    public List<DayOfWeek> QuietDays { get; set; } = new();
}
