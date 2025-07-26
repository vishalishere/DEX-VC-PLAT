using System;
using System.Collections.Generic;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Represents a notification in the system
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Gets or sets the notification ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the user ID the notification is for
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the notification title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the notification message
        /// </summary>
        public string Message { get; set; }
        
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
        /// Gets or sets the expiration date of the notification
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        /// <summary>
        /// Gets or sets additional data related to the notification
        /// </summary>
        public Dictionary<string, string> Data { get; set; }
        
        /// <summary>
        /// Gets or sets the related entity type (e.g., "Project", "Voting", "Transaction")
        /// </summary>
        public string RelatedEntityType { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the related entity
        /// </summary>
        public string RelatedEntityId { get; set; }
        
        /// <summary>
        /// Gets or sets the action URL for the notification
        /// </summary>
        public string ActionUrl { get; set; }
        
        /// <summary>
        /// Gets or sets the action text for the notification
        /// </summary>
        public string ActionText { get; set; }
        
        /// <summary>
        /// Gets or sets the icon for the notification
        /// </summary>
        public string Icon { get; set; }
        
        /// <summary>
        /// Gets or sets whether the notification is a broadcast notification
        /// </summary>
        public bool IsBroadcast { get; set; }
        
        /// <summary>
        /// Gets or sets the priority of the notification (0-10, higher is more important)
        /// </summary>
        public int Priority { get; set; } = 5;
        
        /// <summary>
        /// Gets or sets whether the notification should be delivered immediately
        /// </summary>
        public bool IsUrgent { get; set; }
        
        /// <summary>
        /// Gets or sets the delivery channels for the notification
        /// </summary>
        public List<string> DeliveryChannels { get; set; }
    }
}
