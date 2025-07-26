using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.Common.Analytics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Implementation of the notification service that handles sending notifications to users
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IAnalyticsService _analyticsService;
        private readonly NotificationSettings _settings;
        private readonly ILogger<NotificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class
        /// </summary>
        public NotificationService(
            IMongoClient mongoClient,
            IOptions<NotificationSettings> settings,
            IAnalyticsService analyticsService,
            ILogger<NotificationService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize MongoDB collection
            var database = mongoClient.GetDatabase(_settings.DatabaseName);
            _notifications = database.GetCollection<Notification>(_settings.NotificationsCollectionName);

            // Create indexes for better query performance
            CreateIndexesAsync().GetAwaiter().GetResult();
        }

        /// <inheritdoc />
        public async Task<string> SendNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data = null)
        {
            try
            {
                var notification = CreateNotification(userId, title, message, notificationType, data);
                
                await _notifications.InsertOneAsync(notification);
                
                // Track analytics event
                await TrackNotificationAnalyticsAsync(notification, "NotificationSent");
                
                // Publish to real-time channels if enabled
                if (_settings.EnableRealTimeNotifications)
                {
                    await PublishNotificationAsync(notification);
                }
                
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> SendNotificationToMultipleAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data = null)
        {
            try
            {
                var notifications = new List<Notification>();
                var notificationIds = new List<string>();
                
                foreach (var userId in userIds)
                {
                    var notification = CreateNotification(userId, title, message, notificationType, data);
                    notifications.Add(notification);
                    notificationIds.Add(notification.Id);
                }
                
                if (notifications.Any())
                {
                    await _notifications.InsertManyAsync(notifications);
                    
                    // Track analytics events
                    foreach (var notification in notifications)
                    {
                        await TrackNotificationAnalyticsAsync(notification, "NotificationSent");
                    }
                    
                    // Publish to real-time channels if enabled
                    if (_settings.EnableRealTimeNotifications)
                    {
                        foreach (var notification in notifications)
                        {
                            await PublishNotificationAsync(notification);
                        }
                    }
                }
                
                return notificationIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notifications to multiple users");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendNotificationToRoleAsync(
            string role,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data = null)
        {
            try
            {
                // In a real implementation, we would query the user service to get users with the specified role
                // For now, we'll just log that this would happen
                _logger.LogInformation("Would send notification to users with role {Role}", role);
                
                // Simulate sending to users with the role
                // In a real implementation, this would be replaced with actual user IDs
                var userIds = new List<string> { "simulated-user-1", "simulated-user-2" };
                
                var notificationIds = await SendNotificationToMultipleAsync(
                    userIds,
                    title,
                    message,
                    notificationType,
                    data);
                
                return notificationIds.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to role {Role}", role);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<string> SendBlockchainEventNotificationAsync(
            string userId,
            string transactionHash,
            string eventType,
            string title,
            string message,
            Dictionary<string, string> data = null)
        {
            try
            {
                // Create data dictionary if not provided
                data ??= new Dictionary<string, string>();
                
                // Add blockchain-specific data
                data["TransactionHash"] = transactionHash;
                data["BlockchainEventType"] = eventType;
                
                var notification = CreateNotification(
                    userId,
                    title,
                    message,
                    NotificationType.Blockchain,
                    data);
                
                // Set related entity type and ID
                notification.RelatedEntityType = "BlockchainTransaction";
                notification.RelatedEntityId = transactionHash;
                
                await _notifications.InsertOneAsync(notification);
                
                // Track analytics event
                await TrackNotificationAnalyticsAsync(notification, "BlockchainEventNotification");
                
                // Publish to real-time channels if enabled
                if (_settings.EnableRealTimeNotifications)
                {
                    await PublishNotificationAsync(notification);
                }
                
                return notification.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending blockchain event notification for transaction {TransactionHash}", transactionHash);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendProjectEventNotificationAsync(
            string projectId,
            string eventType,
            string title,
            string message,
            IEnumerable<string> excludeUserIds = null)
        {
            try
            {
                // In a real implementation, we would query the project service to get users interested in the project
                // For now, we'll just log that this would happen
                _logger.LogInformation("Would send notification to users interested in project {ProjectId}", projectId);
                
                // Simulate getting users interested in the project
                // In a real implementation, this would be replaced with actual user IDs
                var interestedUserIds = new List<string> { "simulated-user-1", "simulated-user-2", "simulated-user-3" };
                
                // Exclude specified users
                if (excludeUserIds != null)
                {
                    interestedUserIds = interestedUserIds.Except(excludeUserIds).ToList();
                }
                
                // Create data dictionary
                var data = new Dictionary<string, string>
                {
                    ["ProjectId"] = projectId,
                    ["ProjectEventType"] = eventType
                };
                
                var notificationIds = await SendNotificationToMultipleAsync(
                    interestedUserIds,
                    title,
                    message,
                    NotificationType.Project,
                    data);
                
                return notificationIds.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending project event notification for project {ProjectId}", projectId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> SendVotingEventNotificationAsync(
            string votingSessionId,
            string eventType,
            string title,
            string message,
            IEnumerable<string> excludeUserIds = null)
        {
            try
            {
                // In a real implementation, we would query the voting service to get users participating in the voting session
                // For now, we'll just log that this would happen
                _logger.LogInformation("Would send notification to users participating in voting session {VotingSessionId}", votingSessionId);
                
                // Simulate getting users participating in the voting session
                // In a real implementation, this would be replaced with actual user IDs
                var participatingUserIds = new List<string> { "simulated-user-1", "simulated-user-2", "simulated-user-3" };
                
                // Exclude specified users
                if (excludeUserIds != null)
                {
                    participatingUserIds = participatingUserIds.Except(excludeUserIds).ToList();
                }
                
                // Create data dictionary
                var data = new Dictionary<string, string>
                {
                    ["VotingSessionId"] = votingSessionId,
                    ["VotingEventType"] = eventType
                };
                
                var notificationIds = await SendNotificationToMultipleAsync(
                    participatingUserIds,
                    title,
                    message,
                    NotificationType.Voting,
                    data);
                
                return notificationIds.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending voting event notification for voting session {VotingSessionId}", votingSessionId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            string userId,
            int skip = 0,
            int take = 20,
            bool includeRead = false)
        {
            try
            {
                var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
                
                if (!includeRead)
                {
                    filter = filter & Builders<Notification>.Filter.Eq(n => n.IsRead, false);
                }
                
                var options = new FindOptions<Notification>
                {
                    Sort = Builders<Notification>.Sort.Descending(n => n.CreatedAt),
                    Skip = skip,
                    Limit = take
                };
                
                var notifications = await _notifications.Find(filter).SortByDescending(n => n.CreatedAt)
                    .Skip(skip).Limit(take).ToListAsync();
                
                return notifications;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            try
            {
                var filter = Builders<Notification>.Filter.And(
                    Builders<Notification>.Filter.Eq(n => n.Id, notificationId),
                    Builders<Notification>.Filter.Eq(n => n.UserId, userId));
                
                var update = Builders<Notification>.Update
                    .Set(n => n.IsRead, true)
                    .Set(n => n.ReadAt, DateTime.UtcNow);
                
                var result = await _notifications.UpdateOneAsync(filter, update);
                
                if (result.ModifiedCount > 0)
                {
                    // Track analytics event
                    await _analyticsService.TrackEventAsync(
                        "NotificationRead",
                        userId,
                        new Dictionary<string, string>
                        {
                            ["NotificationId"] = notificationId
                        });
                }
                
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read for user {UserId}", notificationId, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> MarkAllNotificationsAsReadAsync(string userId)
        {
            try
            {
                var filter = Builders<Notification>.Filter.And(
                    Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                    Builders<Notification>.Filter.Eq(n => n.IsRead, false));
                
                var update = Builders<Notification>.Update
                    .Set(n => n.IsRead, true)
                    .Set(n => n.ReadAt, DateTime.UtcNow);
                
                var result = await _notifications.UpdateManyAsync(filter, update);
                
                if (result.ModifiedCount > 0)
                {
                    // Track analytics event
                    await _analyticsService.TrackEventAsync(
                        "AllNotificationsRead",
                        userId,
                        new Dictionary<string, string>
                        {
                            ["Count"] = result.ModifiedCount.ToString()
                        });
                }
                
                return (int)result.ModifiedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            try
            {
                var filter = Builders<Notification>.Filter.And(
                    Builders<Notification>.Filter.Eq(n => n.UserId, userId),
                    Builders<Notification>.Filter.Eq(n => n.IsRead, false));
                
                var count = await _notifications.CountDocumentsAsync(filter);
                
                return (int)count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notification count for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            try
            {
                var filter = Builders<Notification>.Filter.And(
                    Builders<Notification>.Filter.Eq(n => n.Id, notificationId),
                    Builders<Notification>.Filter.Eq(n => n.UserId, userId));
                
                var result = await _notifications.DeleteOneAsync(filter);
                
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting notification {NotificationId} for user {UserId}", notificationId, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> DeleteAllNotificationsAsync(string userId)
        {
            try
            {
                var filter = Builders<Notification>.Filter.Eq(n => n.UserId, userId);
                
                var result = await _notifications.DeleteManyAsync(filter);
                
                return (int)result.DeletedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all notifications for user {UserId}", userId);
                throw;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Creates a new notification object
        /// </summary>
        private Notification CreateNotification(
            string userId,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data)
        {
            return new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = notificationType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = _settings.NotificationExpiryDays > 0 
                    ? DateTime.UtcNow.AddDays(_settings.NotificationExpiryDays) 
                    : null,
                Data = data ?? new Dictionary<string, string>(),
                DeliveryChannels = _settings.DefaultDeliveryChannels?.ToList() ?? new List<string>()
            };
        }

        /// <summary>
        /// Creates MongoDB indexes for better query performance
        /// </summary>
        private async Task CreateIndexesAsync()
        {
            try
            {
                // Create index on UserId for faster user-specific queries
                await _notifications.Indexes.CreateOneAsync(
                    new CreateIndexModel<Notification>(
                        Builders<Notification>.IndexKeys.Ascending(n => n.UserId)));
                
                // Create compound index on UserId and IsRead for faster unread notification queries
                await _notifications.Indexes.CreateOneAsync(
                    new CreateIndexModel<Notification>(
                        Builders<Notification>.IndexKeys
                            .Ascending(n => n.UserId)
                            .Ascending(n => n.IsRead)));
                
                // Create index on CreatedAt for sorting and expiry queries
                await _notifications.Indexes.CreateOneAsync(
                    new CreateIndexModel<Notification>(
                        Builders<Notification>.IndexKeys.Descending(n => n.CreatedAt)));
                
                // Create TTL index on ExpiresAt to automatically delete expired notifications
                await _notifications.Indexes.CreateOneAsync(
                    new CreateIndexModel<Notification>(
                        Builders<Notification>.IndexKeys.Ascending(n => n.ExpiresAt),
                        new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification indexes");
                // Don't throw here, as indexes are for performance and not critical to functionality
            }
        }

        /// <summary>
        /// Tracks a notification-related analytics event
        /// </summary>
        private async Task TrackNotificationAnalyticsAsync(Notification notification, string eventName)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    ["NotificationId"] = notification.Id,
                    ["NotificationType"] = notification.Type.ToString(),
                    ["Title"] = notification.Title
                };
                
                // Add related entity info if available
                if (!string.IsNullOrEmpty(notification.RelatedEntityType))
                {
                    properties["RelatedEntityType"] = notification.RelatedEntityType;
                    properties["RelatedEntityId"] = notification.RelatedEntityId ?? string.Empty;
                }
                
                await _analyticsService.TrackEventAsync(eventName, notification.UserId, properties);
            }
            catch (Exception ex)
            {
                // Log but don't throw, as analytics tracking should not break core functionality
                _logger.LogWarning(ex, "Error tracking notification analytics for event {EventName}", eventName);
            }
        }

        /// <summary>
        /// Publishes a notification to real-time channels
        /// </summary>
        private async Task PublishNotificationAsync(Notification notification)
        {
            try
            {
                // In a real implementation, this would publish to SignalR, WebSockets, or a message bus
                // For now, we'll just log that this would happen
                _logger.LogInformation(
                    "Would publish notification {NotificationId} to real-time channels for user {UserId}",
                    notification.Id,
                    notification.UserId);
                
                // Simulate delay for async processing
                await Task.Delay(10);
            }
            catch (Exception ex)
            {
                // Log but don't throw, as real-time publishing should not break core functionality
                _logger.LogWarning(ex, "Error publishing notification {NotificationId} to real-time channels", notification.Id);
            }
        }

        #endregion
    }
}
