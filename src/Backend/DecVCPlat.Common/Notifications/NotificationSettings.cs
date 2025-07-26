using System.Collections.Generic;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Settings for notification-related functionalities
    /// </summary>
    public class NotificationSettings
    {
        /// <summary>
        /// Gets or sets the MongoDB database name
        /// </summary>
        public string DatabaseName { get; set; } = "DecVCPlatNotifications";
        
        /// <summary>
        /// Gets or sets the MongoDB collection name for notifications
        /// </summary>
        public string NotificationsCollectionName { get; set; } = "Notifications";
        
        /// <summary>
        /// Gets or sets whether real-time notifications are enabled
        /// </summary>
        public bool EnableRealTimeNotifications { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether email notifications are enabled
        /// </summary>
        public bool EnableEmailNotifications { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether push notifications are enabled
        /// </summary>
        public bool EnablePushNotifications { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether SMS notifications are enabled
        /// </summary>
        public bool EnableSmsNotifications { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the number of days after which notifications expire
        /// </summary>
        public int NotificationExpiryDays { get; set; } = 30;
        
        /// <summary>
        /// Gets or sets the maximum number of notifications to keep per user
        /// </summary>
        public int MaxNotificationsPerUser { get; set; } = 100;
        
        /// <summary>
        /// Gets or sets the default delivery channels for notifications
        /// </summary>
        public IEnumerable<string> DefaultDeliveryChannels { get; set; } = new List<string> { "WebApp", "Email" };
        
        /// <summary>
        /// Gets or sets the SMTP server for sending email notifications
        /// </summary>
        public string SmtpServer { get; set; }
        
        /// <summary>
        /// Gets or sets the SMTP port for sending email notifications
        /// </summary>
        public int SmtpPort { get; set; } = 587;
        
        /// <summary>
        /// Gets or sets the SMTP username for sending email notifications
        /// </summary>
        public string SmtpUsername { get; set; }
        
        /// <summary>
        /// Gets or sets the SMTP password for sending email notifications
        /// </summary>
        public string SmtpPassword { get; set; }
        
        /// <summary>
        /// Gets or sets the email address to send notifications from
        /// </summary>
        public string NotificationFromEmail { get; set; }
        
        /// <summary>
        /// Gets or sets the display name for notification emails
        /// </summary>
        public string NotificationFromName { get; set; } = "DecVCPlat Notifications";
        
        /// <summary>
        /// Gets or sets the FCM server key for push notifications
        /// </summary>
        public string FcmServerKey { get; set; }
        
        /// <summary>
        /// Gets or sets the Twilio account SID for SMS notifications
        /// </summary>
        public string TwilioAccountSid { get; set; }
        
        /// <summary>
        /// Gets or sets the Twilio auth token for SMS notifications
        /// </summary>
        public string TwilioAuthToken { get; set; }
        
        /// <summary>
        /// Gets or sets the Twilio phone number for SMS notifications
        /// </summary>
        public string TwilioPhoneNumber { get; set; }
        
        /// <summary>
        /// Gets or sets whether to batch notifications to reduce noise
        /// </summary>
        public bool BatchNotifications { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the batch interval in minutes
        /// </summary>
        public int BatchIntervalMinutes { get; set; } = 15;
        
        /// <summary>
        /// Gets or sets whether to track notification analytics
        /// </summary>
        public bool TrackNotificationAnalytics { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to allow users to customize their notification preferences
        /// </summary>
        public bool AllowUserPreferences { get; set; } = true;
        
        /// <summary>
        /// Gets or sets whether to enforce quiet hours for non-urgent notifications
        /// </summary>
        public bool EnforceQuietHours { get; set; } = true;
        
        /// <summary>
        /// Gets or sets the start hour for quiet hours (24-hour format)
        /// </summary>
        public int QuietHoursStartHour { get; set; } = 22;
        
        /// <summary>
        /// Gets or sets the end hour for quiet hours (24-hour format)
        /// </summary>
        public int QuietHoursEndHour { get; set; } = 7;
    }
}
