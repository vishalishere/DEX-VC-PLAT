namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Enum representing different types of notifications
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// General information notification
        /// </summary>
        Information = 0,
        
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
        /// Project-related notification
        /// </summary>
        Project = 10,
        
        /// <summary>
        /// Voting-related notification
        /// </summary>
        Voting = 20,
        
        /// <summary>
        /// Funding-related notification
        /// </summary>
        Funding = 30,
        
        /// <summary>
        /// Blockchain-related notification
        /// </summary>
        Blockchain = 40,
        
        /// <summary>
        /// Security-related notification
        /// </summary>
        Security = 50,
        
        /// <summary>
        /// User-related notification
        /// </summary>
        User = 60,
        
        /// <summary>
        /// System notification
        /// </summary>
        System = 70
    }
}
