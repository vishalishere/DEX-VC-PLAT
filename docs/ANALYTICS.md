# DecVCPlat Analytics System

This document describes the analytics system implemented in the DecVCPlat platform.

## Overview

The DecVCPlat analytics system provides comprehensive tracking and analysis of platform activities, user behaviors, and project performance. It enables data-driven decision making for platform administrators and provides valuable insights to project owners and investors.

## Architecture

The analytics system consists of several components:

1. **Analytics Service**: Core service for tracking and retrieving analytics events
2. **Event Storage**: MongoDB database for storing analytics events
3. **Application Insights Integration**: Real-time monitoring and telemetry
4. **Analytics API**: REST API for accessing analytics data
5. **Dashboard**: Visual representation of analytics data

## Event Types

The analytics system tracks various types of events:

### User Events
- User registration
- User login
- Profile updates
- User actions (e.g., viewing projects, following projects)

### Project Events
- Project creation
- Project updates
- Project milestones
- Project completion

### Funding Events
- Funding contributions
- Funding goal reached
- Fund releases

### Voting Events
- Voting session creation
- Vote casting
- Voting session finalization

## Data Model

Each analytics event includes:

- **Id**: Unique identifier for the event
- **EventType**: Type of event (e.g., "ProjectCreated", "UserLogin")
- **Category**: Category of event (e.g., "Project", "User", "Funding")
- **UserId**: ID of the user associated with the event (if applicable)
- **Timestamp**: When the event occurred
- **Source**: The source of the event (e.g., microservice name)
- **Properties**: Additional properties as key-value pairs
- **Metrics**: Numeric metrics as key-value pairs

## API Endpoints

### Dashboard Summary

```
GET /api/dashboard/summary?days={days}
```

Returns platform-wide metrics including:
- Total users
- Total projects
- Total funding
- Active users in last 24 hours
- New projects in last 24 hours
- Funding in last 24 hours
- Votes in last 24 hours
- Daily active users over time
- Daily new projects over time
- Daily funding over time

### Project Analytics

```
GET /api/dashboard/projects/{projectId}?days={days}
```

Returns project-specific metrics including:
- Total views
- Unique viewers
- Total funding
- Unique funders
- Total votes
- Daily views over time
- Daily funding over time

### User Analytics

```
GET /api/dashboard/users/{userId}?days={days}
```

Returns user-specific metrics including:
- Projects created
- Projects viewed
- Total funding contributed
- Projects funded
- Total votes cast
- Daily activity over time
- Daily funding over time

## Configuration

Analytics settings are configured in the application's configuration files:

```json
{
  "Analytics": {
    "DatabaseName": "DecVCPlatAnalytics",
    "EventsCollectionName": "Events",
    "EnableRealTimeProcessing": true,
    "TrackAnonymousUsers": true,
    "MaxEventsPerQuery": 1000,
    "SendToApplicationInsights": true
  },
  "ConnectionStrings": {
    "AnalyticsDatabase": "mongodb://localhost:27017"
  }
}
```

## Usage Examples

### Tracking Events

```csharp
// Track a project creation event
await _analyticsService.TrackProjectEventAsync(
    "ProjectCreated",
    projectId,
    userId,
    new Dictionary<string, string>
    {
        { "ProjectName", projectName },
        { "Category", category }
    },
    new Dictionary<string, double>
    {
        { "FundingGoal", (double)fundingGoal }
    });

// Track a funding event
await _analyticsService.TrackFundingEventAsync(
    "ProjectFunded",
    projectId,
    userId,
    amount,
    new Dictionary<string, string>
    {
        { "PaymentMethod", paymentMethod }
    });
```

### Retrieving Analytics Data

```csharp
// Get all events for a specific time period
var events = await _analyticsService.GetEventsAsync(
    startDate,
    endDate,
    "Funding");

// Get events for a specific user
var userEvents = await _analyticsService.GetUserEventsAsync(
    userId,
    startDate,
    endDate);

// Get events for a specific project
var projectEvents = await _analyticsService.GetProjectEventsAsync(
    projectId,
    startDate,
    endDate);
```

## Security Considerations

- User analytics data is only accessible to administrators
- Project analytics data is accessible to project owners and administrators
- Personal identifiable information (PII) is not stored in analytics events
- Access to analytics API endpoints is protected by authentication and authorization

## Dependencies

- **MongoDB**: For storing analytics events
- **Application Insights**: For real-time monitoring and telemetry
- **ASP.NET Core**: For API endpoints
