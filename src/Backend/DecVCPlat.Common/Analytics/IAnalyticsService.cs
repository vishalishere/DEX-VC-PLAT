using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DecVCPlat.Common.Analytics.Models;

namespace DecVCPlat.Common.Analytics
{
    /// <summary>
    /// Interface for analytics service that tracks platform events and metrics
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Tracks an event in the analytics system
        /// </summary>
        /// <param name="eventType">Type of event</param>
        /// <param name="category">Category of event</param>
        /// <param name="userId">User ID associated with the event (optional)</param>
        /// <param name="properties">Additional properties for the event</param>
        /// <param name="metrics">Numeric metrics for the event</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task TrackEventAsync(
            string eventType,
            string category,
            string userId = null,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null);
            
        /// <summary>
        /// Tracks a page view in the analytics system
        /// </summary>
        /// <param name="pageName">Name of the page viewed</param>
        /// <param name="userId">User ID associated with the page view (optional)</param>
        /// <param name="properties">Additional properties for the page view</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task TrackPageViewAsync(
            string pageName,
            string userId = null,
            Dictionary<string, string> properties = null);
            
        /// <summary>
        /// Tracks a user action in the analytics system
        /// </summary>
        /// <param name="actionName">Name of the action performed</param>
        /// <param name="userId">User ID associated with the action</param>
        /// <param name="properties">Additional properties for the action</param>
        /// <param name="metrics">Numeric metrics for the action</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task TrackUserActionAsync(
            string actionName,
            string userId,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null);
            
        /// <summary>
        /// Tracks a project event in the analytics system
        /// </summary>
        /// <param name="eventType">Type of project event</param>
        /// <param name="projectId">Project ID associated with the event</param>
        /// <param name="userId">User ID associated with the event (optional)</param>
        /// <param name="properties">Additional properties for the event</param>
        /// <param name="metrics">Numeric metrics for the event</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task TrackProjectEventAsync(
            string eventType,
            string projectId,
            string userId = null,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null);
            
        /// <summary>
        /// Tracks a funding event in the analytics system
        /// </summary>
        /// <param name="eventType">Type of funding event</param>
        /// <param name="projectId">Project ID associated with the event</param>
        /// <param name="userId">User ID associated with the event</param>
        /// <param name="amount">Funding amount</param>
        /// <param name="properties">Additional properties for the event</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task TrackFundingEventAsync(
            string eventType,
            string projectId,
            string userId,
            decimal amount,
            Dictionary<string, string> properties = null);
            
        /// <summary>
        /// Tracks a voting event in the analytics system
        /// </summary>
        /// <param name="eventType">Type of voting event</param>
        /// <param name="projectId">Project ID associated with the event</param>
        /// <param name="votingSessionId">Voting session ID</param>
        /// <param name="userId">User ID associated with the event</param>
        /// <param name="properties">Additional properties for the event</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task TrackVotingEventAsync(
            string eventType,
            string projectId,
            string votingSessionId,
            string userId,
            Dictionary<string, string> properties = null);
            
        /// <summary>
        /// Gets analytics events for a specific time period
        /// </summary>
        /// <param name="startTime">Start time of the period</param>
        /// <param name="endTime">End time of the period</param>
        /// <param name="category">Optional category filter</param>
        /// <param name="eventType">Optional event type filter</param>
        /// <returns>List of analytics events</returns>
        Task<IEnumerable<AnalyticsEvent>> GetEventsAsync(
            DateTime startTime,
            DateTime endTime,
            string category = null,
            string eventType = null);
            
        /// <summary>
        /// Gets analytics events for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="startTime">Optional start time filter</param>
        /// <param name="endTime">Optional end time filter</param>
        /// <param name="category">Optional category filter</param>
        /// <returns>List of analytics events for the user</returns>
        Task<IEnumerable<AnalyticsEvent>> GetUserEventsAsync(
            string userId,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string category = null);
            
        /// <summary>
        /// Gets analytics events for a specific project
        /// </summary>
        /// <param name="projectId">Project ID</param>
        /// <param name="startTime">Optional start time filter</param>
        /// <param name="endTime">Optional end time filter</param>
        /// <param name="eventType">Optional event type filter</param>
        /// <returns>List of analytics events for the project</returns>
        Task<IEnumerable<AnalyticsEvent>> GetProjectEventsAsync(
            string projectId,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string eventType = null);
    }
}
