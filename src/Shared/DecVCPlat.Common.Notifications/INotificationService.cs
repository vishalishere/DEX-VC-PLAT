using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Interface for notification service operations
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gets notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="skip">Number of notifications to skip</param>
        /// <param name="take">Number of notifications to take</param>
        /// <param name="includeRead">Whether to include read notifications</param>
        /// <returns>A list of notifications</returns>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, int skip = 0, int take = 20, bool includeRead = false);
        
        /// <summary>
        /// Gets the count of unread notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The count of unread notifications</returns>
        Task<int> GetUnreadNotificationCountAsync(string userId);
        
        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">The notification ID</param>
        /// <param name="userId">The user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId);
        
        /// <summary>
        /// Marks all notifications as read for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The number of notifications marked as read</returns>
        Task<int> MarkAllNotificationsAsReadAsync(string userId);
        
        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="notificationId">The notification ID</param>
        /// <param name="userId">The user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);
        
        /// <summary>
        /// Deletes all notifications for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The number of notifications deleted</returns>
        Task<int> DeleteAllNotificationsAsync(string userId);
        
        /// <summary>
        /// Sends a notification to a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="type">The notification type</param>
        /// <param name="data">Additional data for the notification</param>
        /// <returns>The notification ID</returns>
        Task<string> SendNotificationAsync(string userId, string title, string message, NotificationType type, Dictionary<string, string>? data = null);
    }
}
