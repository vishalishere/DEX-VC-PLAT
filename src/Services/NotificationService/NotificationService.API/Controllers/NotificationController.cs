// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.API.Data;
using NotificationService.API.Models.DTOs;
using NotificationService.API.Models.Entities;
using NotificationService.API.Services;
using NotificationService.API.Hubs;
using System.Security.Claims;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly NotificationDbContext _context;
    private readonly IEmailNotificationService _emailService;
    private readonly INotificationHubService _hubService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        NotificationDbContext context,
        IEmailNotificationService emailService,
        INotificationHubService hubService,
        ILogger<NotificationController> logger)
    {
        _context = context;
        _emailService = emailService;
        _hubService = hubService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<ActionResult<NotificationResponse>> CreateNotification([FromBody] CreateNotificationRequest request)
    {
        try
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                UserName = await GetUserNameAsync(request.UserId),
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                ProjectId = request.ProjectId,
                ProposalId = request.ProposalId,
                TrancheId = request.TrancheId,
                MilestoneId = request.MilestoneId,
                SendEmail = request.SendEmail,
                SendPush = request.SendPush,
                SendInApp = request.SendInApp,
                SendSMS = request.SendSMS,
                EmailSubject = request.EmailSubject,
                EmailBody = request.EmailBody,
                ActionUrl = request.ActionUrl,
                ActionText = request.ActionText,
                ExpiresAt = request.ExpiresAt,
                MetadataJson = request.MetadataJson
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time notification
            if (notification.SendInApp)
            {
                await _hubService.SendNotificationToUser(notification.UserId.ToString(), MapToNotificationResponse(notification));
            }

            // Send email notification
            if (notification.SendEmail)
            {
                var userEmail = await GetUserEmailAsync(notification.UserId);
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var emailSent = await _emailService.SendEmailAsync(notification, userEmail, notification.UserName);
                    notification.IsEmailSent = emailSent;
                    notification.EmailSentAt = emailSent ? DateTime.UtcNow : null;
                }
            }

            notification.Status = NotificationStatus.Sent;
            notification.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat notification created and sent: {NotificationId} to user {UserId}", 
                notification.Id, request.UserId);

            return CreatedAtAction(nameof(GetNotification), new { id = notification.Id }, MapToNotificationResponse(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DecVCPlat notification");
            return BadRequest("Failed to create notification");
        }
    }

    [HttpPost("bulk")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<IActionResult> CreateBulkNotifications([FromBody] BulkNotificationRequest request)
    {
        try
        {
            var batchId = Guid.NewGuid();
            var notifications = new List<Notification>();
            var emailNotifications = new List<(Notification notification, string email, string name)>();

            foreach (var userId in request.UserIds)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    UserName = await GetUserNameAsync(userId),
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    Priority = request.Priority,
                    ProjectId = request.ProjectId,
                    ProposalId = request.ProposalId,
                    SendEmail = request.SendEmail,
                    SendPush = request.SendPush,
                    SendInApp = request.SendInApp,
                    BatchId = batchId,
                    IsPartOfBatch = true
                };

                notifications.Add(notification);

                if (notification.SendEmail)
                {
                    var userEmail = await GetUserEmailAsync(userId);
                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        emailNotifications.Add((notification, userEmail, notification.UserName));
                    }
                }
            }

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Send real-time notifications
            if (request.SendInApp)
            {
                var userIds = notifications.Select(n => n.UserId.ToString()).ToList();
                await _hubService.SendBulkNotifications(userIds, new { 
                    title = request.Title, 
                    message = request.Message, 
                    type = request.Type,
                    batchId = batchId
                });
            }

            // Send bulk emails
            if (request.SendEmail && emailNotifications.Any())
            {
                var emailsSent = await _emailService.SendBulkEmailAsync(emailNotifications);
                
                foreach (var notification in notifications.Where(n => n.SendEmail))
                {
                    notification.IsEmailSent = emailsSent;
                    notification.EmailSentAt = emailsSent ? DateTime.UtcNow : null;
                }
            }

            // Update notification statuses
            foreach (var notification in notifications)
            {
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("DecVCPlat bulk notifications created: {Count} notifications with batch ID {BatchId}", 
                notifications.Count, batchId);

            return Ok(new { message = "Bulk notifications sent successfully", batchId, count = notifications.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating DecVCPlat bulk notifications");
            return BadRequest("Failed to create bulk notifications");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NotificationResponse>> GetNotification(Guid id)
    {
        try
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
                return NotFound($"Notification {id} not found");

            var userId = GetCurrentUserId();
            if (notification.UserId != userId && !IsLuminary())
                return Forbid("You can only access your own notifications");

            return Ok(MapToNotificationResponse(notification));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat notification {NotificationId}", id);
            return BadRequest("Failed to retrieve notification");
        }
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<List<NotificationResponse>>> GetUserNotifications(
        Guid userId, 
        [FromQuery] NotificationStatus? status = null,
        [FromQuery] NotificationType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (userId != currentUserId && !IsLuminary())
                return Forbid("You can only access your own notifications");

            var query = _context.Notifications.Where(n => n.UserId == userId);

            if (status.HasValue)
                query = query.Where(n => n.Status == status.Value);
            
            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var responses = notifications.Select(MapToNotificationResponse).ToList();
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat notifications for user {UserId}", userId);
            return BadRequest("Failed to retrieve notifications");
        }
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Unauthorized("User ID not found");

            var notifications = await _context.Notifications
                .Where(n => request.NotificationIds.Contains(n.Id) && n.UserId == userId.Value)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                if (notification.ReadAt == null)
                {
                    notification.ReadAt = DateTime.UtcNow;
                    notification.Status = NotificationStatus.Read;
                }
            }

            await _context.SaveChangesAsync();

            // Notify real-time clients
            await _hubService.SendNotificationToUser(userId.Value.ToString(), new { 
                action = "notifications_read",
                notificationIds = request.NotificationIds
            });

            _logger.LogInformation("DecVCPlat user {UserId} marked {Count} notifications as read", 
                userId.Value, notifications.Count);

            return Ok(new { message = "Notifications marked as read", count = notifications.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking DecVCPlat notifications as read");
            return BadRequest("Failed to mark notifications as read");
        }
    }

    [HttpGet("stats/{userId:guid}")]
    public async Task<ActionResult<NotificationStatsResponse>> GetNotificationStats(Guid userId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (userId != currentUserId && !IsLuminary())
                return Forbid("You can only access your own notification stats");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            var stats = new NotificationStatsResponse
            {
                TotalNotifications = notifications.Count,
                UnreadNotifications = notifications.Count(n => n.ReadAt == null),
                ReadNotifications = notifications.Count(n => n.ReadAt != null),
                PendingNotifications = notifications.Count(n => n.Status == NotificationStatus.Pending),
                SentNotifications = notifications.Count(n => n.Status == NotificationStatus.Sent || n.Status == NotificationStatus.Delivered),
                FailedNotifications = notifications.Count(n => n.Status == NotificationStatus.Failed),
                NotificationsByType = notifications
                    .GroupBy(n => n.Type)
                    .ToDictionary(g => g.Key, g => g.Count()),
                NotificationsByPriority = notifications
                    .GroupBy(n => n.Priority)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving DecVCPlat notification stats for user {UserId}", userId);
            return BadRequest("Failed to retrieve notification stats");
        }
    }

    [HttpPost("project/{projectId:guid}/notify")]
    [Authorize(Policy = "DecVCPlat-Luminary")]
    public async Task<IActionResult> NotifyProjectParticipants(Guid projectId, [FromBody] CreateNotificationRequest request)
    {
        try
        {
            // This would typically get all participants of a project from the Project Management Service
            // For now, we'll create a placeholder implementation
            
            await _hubService.NotifyProjectUpdate(projectId.ToString(), new { 
                title = request.Title,
                message = request.Message,
                type = request.Type,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("DecVCPlat project notification sent for project {ProjectId}", projectId);

            return Ok(new { message = "Project participants notified successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying DecVCPlat project participants for project {ProjectId}", projectId);
            return BadRequest("Failed to notify project participants");
        }
    }

    private async Task<string> GetUserNameAsync(Guid userId)
    {
        // This would typically call the User Management Service
        // For now, return a placeholder
        return $"User_{userId.ToString()[..8]}";
    }

    private async Task<string?> GetUserEmailAsync(Guid userId)
    {
        // This would typically call the User Management Service
        // For now, return a placeholder
        return $"user_{userId.ToString()[..8]}@decvcplat.com";
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("user_id")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private bool IsLuminary()
    {
        return User.FindFirst("role")?.Value == "Luminary";
    }

    private static NotificationResponse MapToNotificationResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            UserName = notification.UserName,
            Title = notification.Title,
            Message = notification.Message,
            Type = notification.Type,
            Priority = notification.Priority,
            Status = notification.Status,
            ProjectId = notification.ProjectId,
            ProposalId = notification.ProposalId,
            TrancheId = notification.TrancheId,
            MilestoneId = notification.MilestoneId,
            SendEmail = notification.SendEmail,
            SendPush = notification.SendPush,
            SendInApp = notification.SendInApp,
            SendSMS = notification.SendSMS,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            DeliveredAt = notification.DeliveredAt,
            ReadAt = notification.ReadAt,
            ExpiresAt = notification.ExpiresAt,
            ActionUrl = notification.ActionUrl,
            ActionText = notification.ActionText,
            IsEmailSent = notification.IsEmailSent,
            IsPushSent = notification.IsPushSent,
            OpenCount = notification.OpenCount,
            ClickCount = notification.ClickCount
        };
    }
}
