using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DecVCPlat.Common.Analytics;
using DecVCPlat.Common.Analytics.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DecVCPlat.Analytics.Controllers
{
    /// <summary>
    /// API controller for analytics dashboard data
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<DashboardController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class
        /// </summary>
        public DashboardController(
            IAnalyticsService analyticsService,
            ILogger<DashboardController> logger)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets summary metrics for the platform dashboard
        /// </summary>
        /// <param name="days">Number of days to include in the summary (default: 30)</param>
        /// <returns>Dashboard summary metrics</returns>
        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetSummary(int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                // Get all events for the period
                var events = await _analyticsService.GetEventsAsync(startDate, endDate);
                
                // Calculate metrics
                var userEvents = events.Where(e => e.Category == "UserActivity").ToList();
                var projectEvents = events.Where(e => e.Category == "Project").ToList();
                var fundingEvents = events.Where(e => e.Category == "Funding").ToList();
                var votingEvents = events.Where(e => e.Category == "Voting").ToList();
                
                // Get unique users
                var uniqueUsers = events
                    .Where(e => !string.IsNullOrEmpty(e.UserId))
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count();
                
                // Get unique projects
                var uniqueProjects = events
                    .Where(e => e.Properties.ContainsKey("ProjectId"))
                    .Select(e => e.Properties["ProjectId"])
                    .Distinct()
                    .Count();
                
                // Calculate total funding
                var totalFunding = fundingEvents
                    .Where(e => e.Metrics.ContainsKey("Amount"))
                    .Sum(e => e.Metrics["Amount"]);
                
                // Get daily active users
                var dailyActiveUsers = userEvents
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Select(e => e.UserId).Distinct().Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                // Get daily new projects
                var dailyNewProjects = projectEvents
                    .Where(e => e.EventType == "ProjectCreated")
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                // Get daily funding
                var dailyFunding = fundingEvents
                    .Where(e => e.EventType == "ProjectFunded" && e.Metrics.ContainsKey("Amount"))
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Sum(e => e.Metrics["Amount"])
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                // Create summary DTO
                var summary = new DashboardSummaryDto
                {
                    TotalUsers = uniqueUsers,
                    TotalProjects = uniqueProjects,
                    TotalFunding = totalFunding,
                    ActiveUsersLast24Hours = userEvents.Count(e => e.Timestamp >= DateTime.UtcNow.AddHours(-24)),
                    NewProjectsLast24Hours = projectEvents.Count(e => e.EventType == "ProjectCreated" && e.Timestamp >= DateTime.UtcNow.AddHours(-24)),
                    FundingLast24Hours = fundingEvents
                        .Where(e => e.EventType == "ProjectFunded" && e.Timestamp >= DateTime.UtcNow.AddHours(-24) && e.Metrics.ContainsKey("Amount"))
                        .Sum(e => e.Metrics["Amount"]),
                    VotesLast24Hours = votingEvents.Count(e => e.EventType == "VoteCast" && e.Timestamp >= DateTime.UtcNow.AddHours(-24)),
                    DailyActiveUsers = dailyActiveUsers,
                    DailyNewProjects = dailyNewProjects,
                    DailyFunding = dailyFunding
                };
                
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard summary for last {Days} days", days);
                return StatusCode(500, "An error occurred while retrieving dashboard data");
            }
        }

        /// <summary>
        /// Gets project analytics data
        /// </summary>
        /// <param name="projectId">ID of the project</param>
        /// <param name="days">Number of days to include (default: 30)</param>
        /// <returns>Project analytics data</returns>
        [HttpGet("projects/{projectId}")]
        public async Task<ActionResult<ProjectAnalyticsDto>> GetProjectAnalytics(string projectId, int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                // Get project events
                var events = await _analyticsService.GetProjectEventsAsync(projectId, startDate);
                
                // Get funding events
                var fundingEvents = events.Where(e => e.Category == "Funding").ToList();
                
                // Get voting events
                var votingEvents = events.Where(e => e.Category == "Voting").ToList();
                
                // Get page view events
                var pageViewEvents = events.Where(e => e.EventType == "PageView").ToList();
                
                // Calculate metrics
                var totalViews = pageViewEvents.Count;
                
                var uniqueViewers = pageViewEvents
                    .Where(e => !string.IsNullOrEmpty(e.UserId))
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count();
                
                var totalFunding = fundingEvents
                    .Where(e => e.Metrics.ContainsKey("Amount"))
                    .Sum(e => e.Metrics["Amount"]);
                
                var uniqueFunders = fundingEvents
                    .Where(e => !string.IsNullOrEmpty(e.UserId))
                    .Select(e => e.UserId)
                    .Distinct()
                    .Count();
                
                var totalVotes = votingEvents.Count(e => e.EventType == "VoteCast");
                
                var dailyViews = pageViewEvents
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                var dailyFunding = fundingEvents
                    .Where(e => e.EventType == "ProjectFunded" && e.Metrics.ContainsKey("Amount"))
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Sum(e => e.Metrics["Amount"])
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                // Create project analytics DTO
                var analytics = new ProjectAnalyticsDto
                {
                    ProjectId = projectId,
                    TotalViews = totalViews,
                    UniqueViewers = uniqueViewers,
                    TotalFunding = totalFunding,
                    UniqueFunders = uniqueFunders,
                    TotalVotes = totalVotes,
                    DailyViews = dailyViews,
                    DailyFunding = dailyFunding
                };
                
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for project {ProjectId} for last {Days} days", projectId, days);
                return StatusCode(500, "An error occurred while retrieving project analytics data");
            }
        }

        /// <summary>
        /// Gets user analytics data
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="days">Number of days to include (default: 30)</param>
        /// <returns>User analytics data</returns>
        [HttpGet("users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserAnalyticsDto>> GetUserAnalytics(string userId, int days = 30)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-days);

                // Get user events
                var events = await _analyticsService.GetUserEventsAsync(userId, startDate);
                
                // Get project events
                var projectEvents = events.Where(e => e.Category == "Project").ToList();
                
                // Get funding events
                var fundingEvents = events.Where(e => e.Category == "Funding").ToList();
                
                // Get voting events
                var votingEvents = events.Where(e => e.Category == "Voting").ToList();
                
                // Calculate metrics
                var projectsCreated = projectEvents.Count(e => e.EventType == "ProjectCreated");
                
                var projectsViewed = events
                    .Where(e => e.EventType == "PageView" && e.Properties.ContainsKey("ProjectId"))
                    .Select(e => e.Properties["ProjectId"])
                    .Distinct()
                    .Count();
                
                var totalFunding = fundingEvents
                    .Where(e => e.Metrics.ContainsKey("Amount"))
                    .Sum(e => e.Metrics["Amount"]);
                
                var projectsFunded = fundingEvents
                    .Where(e => e.EventType == "ProjectFunded")
                    .Select(e => e.Properties["ProjectId"])
                    .Distinct()
                    .Count();
                
                var totalVotes = votingEvents.Count(e => e.EventType == "VoteCast");
                
                var dailyActivity = events
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                var dailyFunding = fundingEvents
                    .Where(e => e.EventType == "ProjectFunded" && e.Metrics.ContainsKey("Amount"))
                    .GroupBy(e => e.Timestamp.Date)
                    .Select(g => new DailyMetricDto
                    {
                        Date = g.Key,
                        Value = g.Sum(e => e.Metrics["Amount"])
                    })
                    .OrderBy(d => d.Date)
                    .ToList();
                
                // Create user analytics DTO
                var analytics = new UserAnalyticsDto
                {
                    UserId = userId,
                    ProjectsCreated = projectsCreated,
                    ProjectsViewed = projectsViewed,
                    TotalFunding = totalFunding,
                    ProjectsFunded = projectsFunded,
                    TotalVotes = totalVotes,
                    DailyActivity = dailyActivity,
                    DailyFunding = dailyFunding
                };
                
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics for user {UserId} for last {Days} days", userId, days);
                return StatusCode(500, "An error occurred while retrieving user analytics data");
            }
        }
    }

    /// <summary>
    /// DTO for dashboard summary data
    /// </summary>
    public class DashboardSummaryDto
    {
        /// <summary>
        /// Total number of users on the platform
        /// </summary>
        public int TotalUsers { get; set; }
        
        /// <summary>
        /// Total number of projects on the platform
        /// </summary>
        public int TotalProjects { get; set; }
        
        /// <summary>
        /// Total funding amount across all projects
        /// </summary>
        public double TotalFunding { get; set; }
        
        /// <summary>
        /// Number of active users in the last 24 hours
        /// </summary>
        public int ActiveUsersLast24Hours { get; set; }
        
        /// <summary>
        /// Number of new projects created in the last 24 hours
        /// </summary>
        public int NewProjectsLast24Hours { get; set; }
        
        /// <summary>
        /// Amount of funding in the last 24 hours
        /// </summary>
        public double FundingLast24Hours { get; set; }
        
        /// <summary>
        /// Number of votes cast in the last 24 hours
        /// </summary>
        public int VotesLast24Hours { get; set; }
        
        /// <summary>
        /// Daily active users over time
        /// </summary>
        public List<DailyMetricDto> DailyActiveUsers { get; set; } = new List<DailyMetricDto>();
        
        /// <summary>
        /// Daily new projects over time
        /// </summary>
        public List<DailyMetricDto> DailyNewProjects { get; set; } = new List<DailyMetricDto>();
        
        /// <summary>
        /// Daily funding over time
        /// </summary>
        public List<DailyMetricDto> DailyFunding { get; set; } = new List<DailyMetricDto>();
    }

    /// <summary>
    /// DTO for project analytics data
    /// </summary>
    public class ProjectAnalyticsDto
    {
        /// <summary>
        /// ID of the project
        /// </summary>
        public string ProjectId { get; set; }
        
        /// <summary>
        /// Total number of views for the project
        /// </summary>
        public int TotalViews { get; set; }
        
        /// <summary>
        /// Number of unique viewers for the project
        /// </summary>
        public int UniqueViewers { get; set; }
        
        /// <summary>
        /// Total funding amount for the project
        /// </summary>
        public double TotalFunding { get; set; }
        
        /// <summary>
        /// Number of unique funders for the project
        /// </summary>
        public int UniqueFunders { get; set; }
        
        /// <summary>
        /// Total number of votes for the project
        /// </summary>
        public int TotalVotes { get; set; }
        
        /// <summary>
        /// Daily views over time
        /// </summary>
        public List<DailyMetricDto> DailyViews { get; set; } = new List<DailyMetricDto>();
        
        /// <summary>
        /// Daily funding over time
        /// </summary>
        public List<DailyMetricDto> DailyFunding { get; set; } = new List<DailyMetricDto>();
    }

    /// <summary>
    /// DTO for user analytics data
    /// </summary>
    public class UserAnalyticsDto
    {
        /// <summary>
        /// ID of the user
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// Number of projects created by the user
        /// </summary>
        public int ProjectsCreated { get; set; }
        
        /// <summary>
        /// Number of projects viewed by the user
        /// </summary>
        public int ProjectsViewed { get; set; }
        
        /// <summary>
        /// Total funding amount contributed by the user
        /// </summary>
        public double TotalFunding { get; set; }
        
        /// <summary>
        /// Number of projects funded by the user
        /// </summary>
        public int ProjectsFunded { get; set; }
        
        /// <summary>
        /// Total number of votes cast by the user
        /// </summary>
        public int TotalVotes { get; set; }
        
        /// <summary>
        /// Daily activity over time
        /// </summary>
        public List<DailyMetricDto> DailyActivity { get; set; } = new List<DailyMetricDto>();
        
        /// <summary>
        /// Daily funding over time
        /// </summary>
        public List<DailyMetricDto> DailyFunding { get; set; } = new List<DailyMetricDto>();
    }

    /// <summary>
    /// DTO for daily metric data
    /// </summary>
    public class DailyMetricDto
    {
        /// <summary>
        /// Date for the metric
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Value for the metric
        /// </summary>
        public double Value { get; set; }
    }
}
