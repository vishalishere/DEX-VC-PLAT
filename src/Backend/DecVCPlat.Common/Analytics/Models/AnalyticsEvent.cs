using System;
using System.Collections.Generic;

namespace DecVCPlat.Common.Analytics.Models
{
    /// <summary>
    /// Represents an analytics event in the system
    /// </summary>
    public class AnalyticsEvent
    {
        /// <summary>
        /// Unique identifier for the event
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// The type of event
        /// </summary>
        public string EventType { get; set; }
        
        /// <summary>
        /// The category of the event
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// The user ID associated with the event (if applicable)
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// The timestamp when the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// The source of the event (e.g., microservice name)
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Additional properties associated with the event
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Numeric metrics associated with the event
        /// </summary>
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
    }
}
