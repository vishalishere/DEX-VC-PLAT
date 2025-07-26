using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.Common.Analytics.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace DecVCPlat.Common.Analytics
{
    /// <summary>
    /// Implementation of the analytics service that tracks platform events and metrics
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IMongoCollection<AnalyticsEvent> _eventsCollection;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly AnalyticsSettings _settings;
        private readonly string _serviceName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsService"/> class
        /// </summary>
        public AnalyticsService(
            IMongoClient mongoClient,
            TelemetryClient telemetryClient,
            IOptions<AnalyticsSettings> settings,
            ILogger<AnalyticsService> logger)
        {
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Get service name from environment or use default
            _serviceName = Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Unknown";
            
            // Initialize MongoDB collection
            var database = mongoClient.GetDatabase(_settings.DatabaseName);
            _eventsCollection = database.GetCollection<AnalyticsEvent>(_settings.EventsCollectionName);
            
            // Create indexes for better query performance
            CreateIndexes();
            
            _logger.LogInformation("Analytics service initialized for service: {ServiceName}", _serviceName);
        }

        /// <inheritdoc />
        public async Task TrackEventAsync(
            string eventType,
            string category,
            string userId = null,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null)
        {
            try
            {
                // Create analytics event
                var analyticsEvent = new AnalyticsEvent
                {
                    EventType = eventType,
                    Category = category,
                    UserId = userId,
                    Source = _serviceName,
                    Properties = properties ?? new Dictionary<string, string>(),
                    Metrics = metrics ?? new Dictionary<string, double>()
                };
                
                // Store in MongoDB
                await _eventsCollection.InsertOneAsync(analyticsEvent);
                
                // Track in Application Insights
                var telemetryEvent = new EventTelemetry(eventType)
                {
                    Timestamp = analyticsEvent.Timestamp
                };
                
                // Add properties
                telemetryEvent.Properties.Add("Category", category);
                telemetryEvent.Properties.Add("Source", _serviceName);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    telemetryEvent.Properties.Add("UserId", userId);
                }
                
                // Add custom properties
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        telemetryEvent.Properties.Add(property.Key, property.Value);
                    }
                }
                
                // Add metrics
                if (metrics != null)
                {
                    foreach (var metric in metrics)
                    {
                        telemetryEvent.Metrics.Add(metric.Key, metric.Value);
                    }
                }
                
                _telemetryClient.TrackEvent(telemetryEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking event {EventType} in category {Category}", eventType, category);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackPageViewAsync(
            string pageName,
            string userId = null,
            Dictionary<string, string> properties = null)
        {
            try
            {
                // Create properties dictionary if null
                var eventProperties = properties ?? new Dictionary<string, string>();
                
                // Add page name to properties
                eventProperties["PageName"] = pageName;
                
                // Track as regular event
                await TrackEventAsync(
                    "PageView",
                    "Navigation",
                    userId,
                    eventProperties);
                
                // Track in Application Insights
                var pageViewTelemetry = new PageViewTelemetry(pageName)
                {
                    Timestamp = DateTime.UtcNow
                };
                
                if (!string.IsNullOrEmpty(userId))
                {
                    pageViewTelemetry.Properties.Add("UserId", userId);
                }
                
                // Add custom properties
                if (properties != null)
                {
                    foreach (var property in properties)
                    {
                        pageViewTelemetry.Properties.Add(property.Key, property.Value);
                    }
                }
                
                _telemetryClient.TrackPageView(pageViewTelemetry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking page view for {PageName}", pageName);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackUserActionAsync(
            string actionName,
            string userId,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID is required for tracking user actions", nameof(userId));
                }
                
                // Create properties dictionary if null
                var eventProperties = properties ?? new Dictionary<string, string>();
                
                // Add action name to properties
                eventProperties["ActionName"] = actionName;
                
                // Track as regular event
                await TrackEventAsync(
                    "UserAction",
                    "UserActivity",
                    userId,
                    eventProperties,
                    metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user action {ActionName} for user {UserId}", actionName, userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackProjectEventAsync(
            string eventType,
            string projectId,
            string userId = null,
            Dictionary<string, string> properties = null,
            Dictionary<string, double> metrics = null)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("Project ID is required for tracking project events", nameof(projectId));
                }
                
                // Create properties dictionary if null
                var eventProperties = properties ?? new Dictionary<string, string>();
                
                // Add project ID to properties
                eventProperties["ProjectId"] = projectId;
                
                // Track as regular event
                await TrackEventAsync(
                    eventType,
                    "Project",
                    userId,
                    eventProperties,
                    metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking project event {EventType} for project {ProjectId}", eventType, projectId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackFundingEventAsync(
            string eventType,
            string projectId,
            string userId,
            decimal amount,
            Dictionary<string, string> properties = null)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("Project ID is required for tracking funding events", nameof(projectId));
                }
                
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID is required for tracking funding events", nameof(userId));
                }
                
                // Create properties dictionary if null
                var eventProperties = properties ?? new Dictionary<string, string>();
                
                // Add project ID to properties
                eventProperties["ProjectId"] = projectId;
                
                // Create metrics dictionary
                var eventMetrics = new Dictionary<string, double>
                {
                    { "Amount", (double)amount }
                };
                
                // Track as regular event
                await TrackEventAsync(
                    eventType,
                    "Funding",
                    userId,
                    eventProperties,
                    eventMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking funding event {EventType} for project {ProjectId}", eventType, projectId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackVotingEventAsync(
            string eventType,
            string projectId,
            string votingSessionId,
            string userId,
            Dictionary<string, string> properties = null)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("Project ID is required for tracking voting events", nameof(projectId));
                }
                
                if (string.IsNullOrEmpty(votingSessionId))
                {
                    throw new ArgumentException("Voting session ID is required for tracking voting events", nameof(votingSessionId));
                }
                
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID is required for tracking voting events", nameof(userId));
                }
                
                // Create properties dictionary if null
                var eventProperties = properties ?? new Dictionary<string, string>();
                
                // Add project ID and voting session ID to properties
                eventProperties["ProjectId"] = projectId;
                eventProperties["VotingSessionId"] = votingSessionId;
                
                // Track as regular event
                await TrackEventAsync(
                    eventType,
                    "Voting",
                    userId,
                    eventProperties);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking voting event {EventType} for session {VotingSessionId}", eventType, votingSessionId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AnalyticsEvent>> GetEventsAsync(
            DateTime startTime,
            DateTime endTime,
            string category = null,
            string eventType = null)
        {
            try
            {
                // Build filter
                var builder = Builders<AnalyticsEvent>.Filter;
                var filter = builder.Gte(e => e.Timestamp, startTime) & builder.Lte(e => e.Timestamp, endTime);
                
                if (!string.IsNullOrEmpty(category))
                {
                    filter &= builder.Eq(e => e.Category, category);
                }
                
                if (!string.IsNullOrEmpty(eventType))
                {
                    filter &= builder.Eq(e => e.EventType, eventType);
                }
                
                // Execute query
                var events = await _eventsCollection.Find(filter)
                    .SortByDescending(e => e.Timestamp)
                    .ToListAsync();
                
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events from {StartTime} to {EndTime}", startTime, endTime);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AnalyticsEvent>> GetUserEventsAsync(
            string userId,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string category = null)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(userId))
                {
                    throw new ArgumentException("User ID is required", nameof(userId));
                }
                
                // Build filter
                var builder = Builders<AnalyticsEvent>.Filter;
                var filter = builder.Eq(e => e.UserId, userId);
                
                if (startTime.HasValue)
                {
                    filter &= builder.Gte(e => e.Timestamp, startTime.Value);
                }
                
                if (endTime.HasValue)
                {
                    filter &= builder.Lte(e => e.Timestamp, endTime.Value);
                }
                
                if (!string.IsNullOrEmpty(category))
                {
                    filter &= builder.Eq(e => e.Category, category);
                }
                
                // Execute query
                var events = await _eventsCollection.Find(filter)
                    .SortByDescending(e => e.Timestamp)
                    .ToListAsync();
                
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events for user {UserId}", userId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AnalyticsEvent>> GetProjectEventsAsync(
            string projectId,
            DateTime? startTime = null,
            DateTime? endTime = null,
            string eventType = null)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("Project ID is required", nameof(projectId));
                }
                
                // Build filter
                var builder = Builders<AnalyticsEvent>.Filter;
                var filter = builder.Eq("Properties.ProjectId", projectId);
                
                if (startTime.HasValue)
                {
                    filter &= builder.Gte(e => e.Timestamp, startTime.Value);
                }
                
                if (endTime.HasValue)
                {
                    filter &= builder.Lte(e => e.Timestamp, endTime.Value);
                }
                
                if (!string.IsNullOrEmpty(eventType))
                {
                    filter &= builder.Eq(e => e.EventType, eventType);
                }
                
                // Execute query
                var events = await _eventsCollection.Find(filter)
                    .SortByDescending(e => e.Timestamp)
                    .ToListAsync();
                
                return events;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving events for project {ProjectId}", projectId);
                throw;
            }
        }

        /// <summary>
        /// Creates indexes for better query performance
        /// </summary>
        private void CreateIndexes()
        {
            try
            {
                // Create index on timestamp
                var timestampIndexModel = new CreateIndexModel<AnalyticsEvent>(
                    Builders<AnalyticsEvent>.IndexKeys.Descending(e => e.Timestamp));
                
                // Create index on user ID
                var userIdIndexModel = new CreateIndexModel<AnalyticsEvent>(
                    Builders<AnalyticsEvent>.IndexKeys.Ascending(e => e.UserId));
                
                // Create index on category and event type
                var categoryEventTypeIndexModel = new CreateIndexModel<AnalyticsEvent>(
                    Builders<AnalyticsEvent>.IndexKeys
                        .Ascending(e => e.Category)
                        .Ascending(e => e.EventType));
                
                // Create compound index on timestamp and category
                var timestampCategoryIndexModel = new CreateIndexModel<AnalyticsEvent>(
                    Builders<AnalyticsEvent>.IndexKeys
                        .Descending(e => e.Timestamp)
                        .Ascending(e => e.Category));
                
                // Create indexes
                _eventsCollection.Indexes.CreateMany(new[]
                {
                    timestampIndexModel,
                    userIdIndexModel,
                    categoryEventTypeIndexModel,
                    timestampCategoryIndexModel
                });
                
                _logger.LogInformation("Created indexes for analytics events collection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating indexes for analytics events collection");
                // Don't throw, as this is not critical for the service to function
            }
        }
    }
}
