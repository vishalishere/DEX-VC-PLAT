using System;
using System.Collections.Generic;

namespace DecVCPlat.Common.Notifications.New
{
    /// <summary>
    /// Represents a notification in the system
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the notification ID
        /// </summary>
        public string Id { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the user ID the notification is for
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the notification title
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the notification message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the notification type
        /// </summary>
        public NotificationType Type { get; set; }
        
        /// <summary>
        /// Gets or sets whether the notification has been read
        /// </summary>
        public bool IsRead { get; set; }
        
        /// <summary>
        /// Gets or sets the creation date of the notification
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the notification was read
        /// </summary>
        public DateTime? ReadAt { get; set; }
        
        /// <summary>
        /// Gets or sets additional data for the notification
        /// </summary>
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
