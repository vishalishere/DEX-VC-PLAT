namespace DecVCPlat.Common.Analytics
{
    /// <summary>
    /// Settings for the analytics service
    /// </summary>
    public class AnalyticsSettings
    {
        /// <summary>
        /// The name of the MongoDB database for analytics
        /// </summary>
        public string DatabaseName { get; set; } = "DecVCPlatAnalytics";
        
        /// <summary>
        /// The name of the MongoDB collection for analytics events
        /// </summary>
        public string EventsCollectionName { get; set; } = "Events";
        
        /// <summary>
        /// Whether to enable real-time analytics processing
        /// </summary>
        public bool EnableRealTimeProcessing { get; set; } = true;
        
        /// <summary>
        /// Whether to track anonymous users
        /// </summary>
        public bool TrackAnonymousUsers { get; set; } = true;
        
        /// <summary>
        /// Maximum number of events to return in a single query
        /// </summary>
        public int MaxEventsPerQuery { get; set; } = 1000;
        
        /// <summary>
        /// Whether to send analytics data to Application Insights
        /// </summary>
        public bool SendToApplicationInsights { get; set; } = true;
    }
}
