using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Default implementation of the notification service
    /// </summary>
    public class DefaultNotificationService : INotificationService
    {
        private readonly ILogger<DefaultNotificationService> _logger;
        private readonly Dictionary<string, List<Notification>> _userNotifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultNotificationService"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public DefaultNotificationService(ILogger<DefaultNotificationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userNotifications = new Dictionary<string, List<Notification>>();
        }

        /// <inheritdoc />
        public Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, bool includeRead = false)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(Enumerable.Empty<Notification>());
            }

            var query = notifications.AsEnumerable();
            
            if (!includeRead)
            {
                query = query.Where(n => !n.IsRead);
            }
            
            query = query.OrderByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Take(take);
            
            return Task.FromResult(query);
        }

        /// <inheritdoc />
        public Task<int> GetUnreadNotificationCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(0);
            }

            return Task.FromResult(notifications.Count(n => !n.IsRead));
        }

        /// <inheritdoc />
        public Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                throw new ArgumentException("Notification ID cannot be null or empty", nameof(notificationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(false);
            }

            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification == null)
            {
                return Task.FromResult(false);
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> MarkAllNotificationsAsReadAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(0);
            }

            var count = 0;
            var now = DateTime.UtcNow;
            
            foreach (var notification in notifications.Where(n => !n.IsRead))
            {
                notification.IsRead = true;
                notification.ReadAt = now;
                count++;
            }
            
            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                throw new ArgumentException("Notification ID cannot be null or empty", nameof(notificationId));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(false);
            }

            var notification = notifications.FirstOrDefault(n => n.Id == notificationId);
            if (notification == null)
            {
                return Task.FromResult(false);
            }

            notifications.Remove(notification);
            
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        public Task<int> DeleteAllNotificationsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                return Task.FromResult(0);
            }

            var count = notifications.Count;
            notifications.Clear();
            
            return Task.FromResult(count);
        }

        /// <inheritdoc />
        public Task<string> SendNotificationAsync(string userId, string title, string message, NotificationType type, Dictionary<string, string>? data = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title cannot be null or empty", nameof(title));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            }

            if (!_userNotifications.TryGetValue(userId, out var notifications))
            {
                notifications = new List<Notification>();
                _userNotifications[userId] = notifications;
            }

            var notification = new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                Data = data ?? new Dictionary<string, string>()
            };
            
            notifications.Add(notification);
            _logger.LogInformation($"Notification sent to user {userId}: {title}");
            
            return Task.FromResult(notification.Id);
        }
    }
}
