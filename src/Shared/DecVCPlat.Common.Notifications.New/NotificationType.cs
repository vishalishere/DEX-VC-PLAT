namespace DecVCPlat.Common.Notifications.New
{
    /// <summary>
    /// Represents the type of notification
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// General information notification
        /// </summary>
        Info = 0,
        
        /// <summary>
        /// Success notification
        /// </summary>
        Success = 1,
        
        /// <summary>
        /// Warning notification
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// Error notification
        /// </summary>
        Error = 3,
        
        /// <summary>
        /// Project update notification
        /// </summary>
        ProjectUpdate = 10,
        
        /// <summary>
        /// Voting notification
        /// </summary>
        Voting = 20,
        
        /// <summary>
        /// Funding notification
        /// </summary>
        Funding = 30,
        
        /// <summary>
        /// User mention notification
        /// </summary>
        UserMention = 40,
        
        /// <summary>
        /// System notification
        /// </summary>
        System = 100
    }
}
