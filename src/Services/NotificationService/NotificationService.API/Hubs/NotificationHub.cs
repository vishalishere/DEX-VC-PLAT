// © 2024 DecVCPlat. All rights reserved.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace NotificationService.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? Context.User?.FindFirst("user_id")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal notification group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            
            // Add user to role-based groups
            var userRole = Context.User?.FindFirst("role")?.Value;
            if (!string.IsNullOrEmpty(userRole))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{userRole.ToLower()}");
            }

            _logger.LogInformation("DecVCPlat user {UserId} connected to notification hub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                     ?? Context.User?.FindFirst("user_id")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("DecVCPlat user {UserId} disconnected from notification hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Join project-specific groups for project updates
    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
        
        var userId = GetCurrentUserId();
        _logger.LogInformation("DecVCPlat user {UserId} joined project group {ProjectId}", userId, projectId);
    }

    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
        
        var userId = GetCurrentUserId();
        _logger.LogInformation("DecVCPlat user {UserId} left project group {ProjectId}", userId, projectId);
    }

    // Join proposal-specific groups for voting updates
    public async Task JoinProposal(string proposalId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"proposal_{proposalId}");
        
        var userId = GetCurrentUserId();
        _logger.LogInformation("DecVCPlat user {UserId} joined proposal group {ProposalId}", userId, proposalId);
    }

    public async Task LeaveProposal(string proposalId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"proposal_{proposalId}");
        
        var userId = GetCurrentUserId();
        _logger.LogInformation("DecVCPlat user {UserId} left proposal group {ProposalId}", userId, proposalId);
    }

    // Mark notifications as read
    public async Task MarkNotificationsAsRead(List<string> notificationIds)
    {
        var userId = GetCurrentUserId();
        
        // This would typically update the database
        // For now, we'll just acknowledge the action
        await Clients.Caller.SendAsync("NotificationsMarkedAsRead", notificationIds);
        
        _logger.LogInformation("DecVCPlat user {UserId} marked {Count} notifications as read", userId, notificationIds.Count);
    }

    // Update notification preferences
    public async Task UpdatePreferences(object preferences)
    {
        var userId = GetCurrentUserId();
        
        // This would typically update user preferences in the database
        await Clients.Caller.SendAsync("PreferencesUpdated", preferences);
        
        _logger.LogInformation("DecVCPlat user {UserId} updated notification preferences", userId);
    }

    private string? GetCurrentUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? Context.User?.FindFirst("user_id")?.Value;
    }
}

// Service for sending real-time notifications via SignalR
public interface INotificationHubService
{
    Task SendNotificationToUser(string userId, object notification);
    Task SendNotificationToGroup(string groupName, object notification);
    Task SendBulkNotifications(List<string> userIds, object notification);
    Task NotifyProjectUpdate(string projectId, object update);
    Task NotifyProposalUpdate(string proposalId, object update);
    Task NotifyRoleGroup(string role, object notification);
}

public class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationHubService> _logger;

    public NotificationHubService(IHubContext<NotificationHub> hubContext, ILogger<NotificationHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUser(string userId, object notification)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("DecVCPlat real-time notification sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat real-time notification to user {UserId}", userId);
        }
    }

    public async Task SendNotificationToGroup(string groupName, object notification)
    {
        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("DecVCPlat real-time notification sent to group {GroupName}", groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat real-time notification to group {GroupName}", groupName);
        }
    }

    public async Task SendBulkNotifications(List<string> userIds, object notification)
    {
        try
        {
            var groupNames = userIds.Select(id => $"user_{id}").ToArray();
            await _hubContext.Clients.Groups(groupNames).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("DecVCPlat bulk real-time notification sent to {UserCount} users", userIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat bulk real-time notifications");
        }
    }

    public async Task NotifyProjectUpdate(string projectId, object update)
    {
        try
        {
            await _hubContext.Clients.Group($"project_{projectId}").SendAsync("ProjectUpdate", update);
            _logger.LogInformation("DecVCPlat project update sent for project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat project update for project {ProjectId}", projectId);
        }
    }

    public async Task NotifyProposalUpdate(string proposalId, object update)
    {
        try
        {
            await _hubContext.Clients.Group($"proposal_{proposalId}").SendAsync("ProposalUpdate", update);
            _logger.LogInformation("DecVCPlat proposal update sent for proposal {ProposalId}", proposalId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat proposal update for proposal {ProposalId}", proposalId);
        }
    }

    public async Task NotifyRoleGroup(string role, object notification)
    {
        try
        {
            await _hubContext.Clients.Group($"role_{role.ToLower()}").SendAsync("RoleNotification", notification);
            _logger.LogInformation("DecVCPlat role notification sent to {Role} group", role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat role notification to {Role} group", role);
        }
    }
}
