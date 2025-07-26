# DecVCPlat Notification System

This document describes the notification system implemented in the DecVCPlat platform.

## Overview

The DecVCPlat notification system provides real-time and asynchronous notifications to users about important events occurring within the platform. It integrates with the blockchain and analytics systems to provide comprehensive event tracking and user engagement.

## Architecture

The notification system consists of several components:

1. **Notification Service**: Core service for creating, managing, and delivering notifications
2. **Notification Storage**: MongoDB database for storing notification data
3. **Delivery Channels**: Multiple channels for notification delivery (WebApp, Email, Push, SMS)
4. **Real-time Notifications**: Immediate delivery of important notifications
5. **Analytics Integration**: Tracking of notification-related events

## Notification Types

The system supports various notification types:

- **Information**: General informational notifications
- **Success**: Notifications about successful operations
- **Warning**: Notifications about potential issues
- **Error**: Notifications about errors or failures
- **Project**: Project-related notifications
- **Voting**: Voting-related notifications
- **Funding**: Funding-related notifications
- **Blockchain**: Blockchain transaction and event notifications
- **Security**: Security-related notifications
- **User**: User-related notifications
- **System**: System-wide notifications

## Data Model

Each notification includes:

- **Id**: Unique identifier for the notification
- **UserId**: ID of the user the notification is for
- **Title**: Short title of the notification
- **Message**: Detailed message content
- **Type**: The notification type
- **IsRead**: Whether the notification has been read
- **CreatedAt**: When the notification was created
- **ReadAt**: When the notification was read (if applicable)
- **ExpiresAt**: When the notification expires
- **Data**: Additional data as key-value pairs
- **RelatedEntityType**: Type of related entity (e.g., "Project", "Voting")
- **RelatedEntityId**: ID of the related entity
- **ActionUrl**: URL for the user to take action
- **ActionText**: Text for the action button
- **Icon**: Icon to display with the notification
- **IsBroadcast**: Whether it's a broadcast notification
- **Priority**: Priority level (0-10)
- **IsUrgent**: Whether the notification is urgent
- **DeliveryChannels**: Channels to deliver the notification through

## Features

### Real-time Notifications

The system supports real-time notifications through WebSockets or SignalR, allowing users to receive immediate updates about important events.

### Multi-channel Delivery

Notifications can be delivered through multiple channels:

- **WebApp**: In-app notifications
- **Email**: Email notifications
- **Push**: Mobile push notifications
- **SMS**: Text message notifications

### Blockchain Event Notifications

The system integrates with the blockchain system to notify users about:

- Transaction confirmations
- Smart contract events
- Voting results
- Fund releases
- Project milestones

### Analytics Integration

The notification system integrates with the analytics system to track:

- Notification deliveries
- Open rates
- Click-through rates
- User engagement

### Batching and Throttling

To prevent notification fatigue, the system supports:

- Batching similar notifications
- Enforcing quiet hours
- User-defined notification preferences

## API Endpoints

### Get User Notifications

```
GET /api/notifications?skip={skip}&take={take}&includeRead={includeRead}
```

Returns notifications for the authenticated user.

### Mark Notification as Read

```
PUT /api/notifications/{id}/read
```

Marks a notification as read.

### Mark All Notifications as Read

```
PUT /api/notifications/read-all
```

Marks all notifications for the authenticated user as read.

### Delete Notification

```
DELETE /api/notifications/{id}
```

Deletes a notification.

### Get Unread Count

```
GET /api/notifications/unread-count
```

Returns the count of unread notifications for the authenticated user.

## Configuration

Notification settings are configured in the application's configuration files:

```json
{
  "Notifications": {
    "DatabaseName": "DecVCPlatNotifications",
    "NotificationsCollectionName": "Notifications",
    "EnableRealTimeNotifications": true,
    "EnableEmailNotifications": true,
    "EnablePushNotifications": true,
    "EnableSmsNotifications": false,
    "NotificationExpiryDays": 30,
    "MaxNotificationsPerUser": 100,
    "DefaultDeliveryChannels": ["WebApp", "Email"],
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "notifications@example.com",
    "SmtpPassword": "your-smtp-password",
    "NotificationFromEmail": "notifications@example.com",
    "NotificationFromName": "DecVCPlat Notifications",
    "BatchNotifications": true,
    "BatchIntervalMinutes": 15,
    "TrackNotificationAnalytics": true,
    "AllowUserPreferences": true,
    "EnforceQuietHours": true,
    "QuietHoursStartHour": 22,
    "QuietHoursEndHour": 7
  },
  "ConnectionStrings": {
    "NotificationsDatabase": "mongodb://localhost:27017"
  }
}
```

## Usage Examples

### Registering Notification Services

```csharp
// In Startup.cs or Program.cs
services.AddNotificationServices(Configuration);
```

### Sending a Notification

```csharp
// Send a notification to a user
await _notificationService.SendNotificationAsync(
    userId,
    "Project Approved",
    "Your project has been approved by the community.",
    NotificationType.Project,
    new Dictionary<string, string>
    {
        { "ProjectId", projectId },
        { "ProjectName", projectName }
    });
```

### Sending a Blockchain Event Notification

```csharp
// Send a blockchain event notification
await _notificationService.SendBlockchainEventNotificationAsync(
    userId,
    transactionHash,
    "TransactionConfirmed",
    "Transaction Confirmed",
    "Your transaction has been confirmed on the blockchain.",
    new Dictionary<string, string>
    {
        { "BlockNumber", blockNumber.ToString() },
        { "ConfirmationCount", confirmations.ToString() }
    });
```

### Getting User Notifications

```csharp
// Get unread notifications for a user
var notifications = await _notificationService.GetUserNotificationsAsync(
    userId,
    skip: 0,
    take: 20,
    includeRead: false);
```

### Marking Notifications as Read

```csharp
// Mark a notification as read
await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);

// Mark all notifications as read
await _notificationService.MarkAllNotificationsAsReadAsync(userId);
```

## Integration with Other Systems

### Blockchain Integration

The notification system integrates with the blockchain system to notify users about blockchain events:

```csharp
// In a blockchain event handler
public async Task HandleBlockchainEventAsync(BlockchainEvent @event)
{
    // Notify affected users
    await _notificationService.SendBlockchainEventNotificationAsync(
        @event.UserId,
        @event.TransactionHash,
        @event.EventType,
        "Blockchain Event",
        @event.Description,
        @event.AdditionalData);
}
```

### Analytics Integration

The notification system integrates with the analytics system to track notification-related events:

```csharp
// In NotificationService.cs
private async Task TrackNotificationAnalyticsAsync(Notification notification, string eventName)
{
    await _analyticsService.TrackEventAsync(
        eventName,
        notification.UserId,
        new Dictionary<string, string>
        {
            ["NotificationId"] = notification.Id,
            ["NotificationType"] = notification.Type.ToString()
        });
}
```

## Security Considerations

- Notification data is stored securely in MongoDB
- User-specific notifications are only accessible to the intended recipient
- Sensitive data is not included in notification content
- Authentication and authorization are required for all notification endpoints

## Dependencies

- **MongoDB.Driver**: For storing notification data
- **Microsoft.Extensions.Options**: For configuration
- **Microsoft.Extensions.Logging**: For logging
- **DecVCPlat.Common.Analytics**: For analytics integration
