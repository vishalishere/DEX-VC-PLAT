// © 2024 DecVCPlat. All rights reserved.

using SendGrid;
using SendGrid.Helpers.Mail;
using NotificationService.API.Models.Entities;

namespace NotificationService.API.Services;

public interface IEmailNotificationService
{
    Task<bool> SendEmailAsync(Notification notification, string recipientEmail, string recipientName);
    Task<bool> SendBulkEmailAsync(List<(Notification notification, string email, string name)> notifications);
}

public class EmailNotificationService : IEmailNotificationService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(ISendGridClient sendGridClient, IConfiguration configuration, ILogger<EmailNotificationService> logger)
    {
        _sendGridClient = sendGridClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(Notification notification, string recipientEmail, string recipientName)
    {
        try
        {
            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@decvcplat.com";
            var fromName = _configuration["SendGrid:FromName"] ?? "DecVCPlat";

            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(recipientEmail, recipientName);
            
            var subject = notification.EmailSubject ?? notification.Title;
            var plainTextContent = notification.Message;
            var htmlContent = GenerateEmailHtml(notification);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            
            // Add custom headers for tracking
            msg.AddCustomArg("notification_id", notification.Id.ToString());
            msg.AddCustomArg("notification_type", notification.Type.ToString());
            msg.AddCustomArg("user_id", notification.UserId.ToString());

            // Add categories for SendGrid analytics
            msg.AddCategory("DecVCPlat");
            msg.AddCategory($"NotificationType_{notification.Type}");
            msg.AddCategory($"Priority_{notification.Priority}");

            var response = await _sendGridClient.SendEmailAsync(msg);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Email sent successfully for DecVCPlat notification {NotificationId} to {Email}", 
                    notification.Id, recipientEmail);
                return true;
            }
            else
            {
                var responseBody = await response.Body.ReadAsStringAsync();
                _logger.LogError("Failed to send DecVCPlat email for notification {NotificationId}: {StatusCode} - {Response}", 
                    notification.Id, response.StatusCode, responseBody);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending DecVCPlat email for notification {NotificationId} to {Email}", 
                notification.Id, recipientEmail);
            return false;
        }
    }

    public async Task<bool> SendBulkEmailAsync(List<(Notification notification, string email, string name)> notifications)
    {
        try
        {
            var fromEmail = _configuration["SendGrid:FromEmail"] ?? "noreply@decvcplat.com";
            var fromName = _configuration["SendGrid:FromName"] ?? "DecVCPlat";

            var from = new EmailAddress(fromEmail, fromName);
            
            var messages = new List<SendGridMessage>();
            
            foreach (var (notification, email, name) in notifications)
            {
                var to = new EmailAddress(email, name);
                var subject = notification.EmailSubject ?? notification.Title;
                var plainTextContent = notification.Message;
                var htmlContent = GenerateEmailHtml(notification);

                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                
                // Add custom headers for tracking
                msg.AddCustomArg("notification_id", notification.Id.ToString());
                msg.AddCustomArg("notification_type", notification.Type.ToString());
                msg.AddCustomArg("user_id", notification.UserId.ToString());
                msg.AddCustomArg("batch_id", notification.BatchId?.ToString() ?? "");

                // Add categories
                msg.AddCategory("DecVCPlat");
                msg.AddCategory($"NotificationType_{notification.Type}");
                msg.AddCategory("BulkEmail");

                messages.Add(msg);
            }

            // Send emails in batches of 100 (SendGrid limit)
            var batches = messages.Chunk(100);
            var allSuccessful = true;

            foreach (var batch in batches)
            {
                try
                {
                    var responses = await Task.WhenAll(batch.Select(msg => _sendGridClient.SendEmailAsync(msg)));
                    
                    var failedCount = responses.Count(r => !r.IsSuccessStatusCode);
                    if (failedCount > 0)
                    {
                        allSuccessful = false;
                        _logger.LogWarning("DecVCPlat bulk email batch had {FailedCount} failures out of {TotalCount}", 
                            failedCount, batch.Count());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending DecVCPlat bulk email batch");
                    allSuccessful = false;
                }
            }

            _logger.LogInformation("DecVCPlat bulk email completed: {TotalEmails} emails processed", notifications.Count);
            return allSuccessful;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DecVCPlat bulk email operation");
            return false;
        }
    }

    private string GenerateEmailHtml(Notification notification)
    {
        var actionButton = string.Empty;
        if (!string.IsNullOrEmpty(notification.ActionUrl) && !string.IsNullOrEmpty(notification.ActionText))
        {
            actionButton = $@"
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""{notification.ActionUrl}"" 
                       style=""background-color: #007bff; color: white; padding: 12px 24px; 
                              text-decoration: none; border-radius: 4px; display: inline-block;"">
                        {notification.ActionText}
                    </a>
                </div>";
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{notification.Title}</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin-bottom: 20px;"">
        <h1 style=""color: #007bff; margin: 0; font-size: 24px;"">DecVCPlat</h1>
        <p style=""margin: 5px 0 0 0; color: #666;"">Decentralized Venture Capital Platform</p>
    </div>
    
    <div style=""background-color: white; padding: 20px; border-radius: 8px; border: 1px solid #e9ecef;"">
        <h2 style=""color: #333; margin-top: 0;"">{notification.Title}</h2>
        <p style=""margin-bottom: 20px;"">{notification.Message}</p>
        
        {actionButton}
        
        <div style=""border-top: 1px solid #e9ecef; padding-top: 15px; margin-top: 20px; font-size: 12px; color: #666;"">
            <p>This notification was sent from DecVCPlat on {notification.CreatedAt:yyyy-MM-dd HH:mm} UTC.</p>
            <p>If you no longer wish to receive these notifications, you can update your preferences in your account settings.</p>
        </div>
    </div>
    
    <div style=""text-align: center; margin-top: 20px; font-size: 12px; color: #666;"">
        <p>&copy; 2024 DecVCPlat. All rights reserved.</p>
    </div>
</body>
</html>";
    }
}
