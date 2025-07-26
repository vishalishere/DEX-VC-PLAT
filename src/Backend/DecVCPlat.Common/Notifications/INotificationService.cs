using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Interface for notification service that handles sending notifications to users
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification to a specific user
        /// </summary>
        /// <param name="userId">The ID of the user to notify</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="notificationType">The type of notification</param>
        /// <param name="data">Additional data related to the notification</param>
        /// <returns>The ID of the created notification</returns>
        Task<string> SendNotificationAsync(
            string userId,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data = null);

        /// <summary>
        /// Sends a notification to multiple users
        /// </summary>
        /// <param name="userIds">The IDs of the users to notify</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="notificationType">The type of notification</param>
        /// <param name="data">Additional data related to the notification</param>
        /// <returns>The IDs of the created notifications</returns>
        Task<IEnumerable<string>> SendNotificationToMultipleAsync(
            IEnumerable<string> userIds,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data = null);

        /// <summary>
        /// Sends a notification to all users with a specific role
        /// </summary>
        /// <param name="role">The role of users to notify</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="notificationType">The type of notification</param>
        /// <param name="data">Additional data related to the notification</param>
        /// <returns>The number of users notified</returns>
        Task<int> SendNotificationToRoleAsync(
            string role,
            string title,
            string message,
            NotificationType notificationType,
            Dictionary<string, string> data = null);

        /// <summary>
        /// Sends a notification about a blockchain event
        /// </summary>
        /// <param name="userId">The ID of the user to notify (null for broadcast)</param>
        /// <param name="transactionHash">The blockchain transaction hash</param>
        /// <param name="eventType">The type of blockchain event</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="data">Additional data related to the notification</param>
        /// <returns>The ID of the created notification</returns>
        Task<string> SendBlockchainEventNotificationAsync(
            string userId,
            string transactionHash,
            string eventType,
            string title,
            string message,
            Dictionary<string, string> data = null);

        /// <summary>
        /// Sends a notification about a project event
        /// </summary>
        /// <param name="projectId">The ID of the project</param>
        /// <param name="eventType">The type of project event</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="excludeUserIds">User IDs to exclude from notification</param>
        /// <returns>The number of users notified</returns>
        Task<int> SendProjectEventNotificationAsync(
            string projectId,
            string eventType,
            string title,
            string message,
            IEnumerable<string> excludeUserIds = null);

        /// <summary>
        /// Sends a notification about a voting event
        /// </summary>
        /// <param name="votingSessionId">The ID of the voting session</param>
        /// <param name="eventType">The type of voting event</param>
        /// <param name="title">The notification title</param>
        /// <param name="message">The notification message</param>
        /// <param name="excludeUserIds">User IDs to exclude from notification</param>
        /// <returns>The number of users notified</returns>
        Task<int> SendVotingEventNotificationAsync(
            string votingSessionId,
            string eventType,
            string title,
            string message,
            IEnumerable<string> excludeUserIds = null);

        /// <summary>
        /// Gets notifications for a specific user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="skip">Number of notifications to skip</param>
        /// <param name="take">Number of notifications to take</param>
        /// <param name="includeRead">Whether to include read notifications</param>
        /// <returns>The notifications for the user</returns>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(
            string userId,
            int skip = 0,
            int take = 20,
            bool includeRead = false);

        /// <summary>
        /// Marks a notification as read
        /// </summary>
        /// <param name="notificationId">The ID of the notification</param>
        /// <param name="userId">The ID of the user</param>
        /// <returns>True if the notification was marked as read, false otherwise</returns>
        Task<bool> MarkNotificationAsReadAsync(string notificationId, string userId);

        /// <summary>
        /// Marks all notifications for a user as read
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The number of notifications marked as read</returns>
        Task<int> MarkAllNotificationsAsReadAsync(string userId);

        /// <summary>
        /// Gets the count of unread notifications for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The count of unread notifications</returns>
        Task<int> GetUnreadNotificationCountAsync(string userId);

        /// <summary>
        /// Deletes a notification
        /// </summary>
        /// <param name="notificationId">The ID of the notification</param>
        /// <param name="userId">The ID of the user</param>
        /// <returns>True if the notification was deleted, false otherwise</returns>
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);

        /// <summary>
        /// Deletes all notifications for a user
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <returns>The number of notifications deleted</returns>
        Task<int> DeleteAllNotificationsAsync(string userId);
    }
}
