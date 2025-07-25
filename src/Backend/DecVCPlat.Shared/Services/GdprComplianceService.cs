// © 2024 DecVCPlat. All rights reserved.

using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DecVCPlat.Shared.Services;

public interface IDecVCPlatGdprComplianceService
{
    Task<DecVCPlatDataExportResult> ExportUserDataAsync(string userId);
    Task<bool> DeleteUserDataAsync(string userId, bool confirmDeletion = false);
    Task<bool> ProcessDataSubjectRequestAsync(DecVCPlatDataSubjectRequest request);
    Task<bool> RecordConsentAsync(string userId, DecVCPlatConsentType consentType, bool granted);
    Task<DecVCPlatConsentStatus> GetConsentStatusAsync(string userId);
}

public class DecVCPlatGdprComplianceService : IDecVCPlatGdprComplianceService
{
    private readonly ILogger<DecVCPlatGdprComplianceService> _logger;
    private readonly IDecVCPlatDataRepository _dataRepository;
    private readonly IDecVCPlatAuditLogger _auditLogger;

    public DecVCPlatGdprComplianceService(
        ILogger<DecVCPlatGdprComplianceService> logger,
        IDecVCPlatDataRepository dataRepository,
        IDecVCPlatAuditLogger auditLogger)
    {
        _logger = logger;
        _dataRepository = dataRepository;
        _auditLogger = auditLogger;
    }

    public async Task<DecVCPlatDataExportResult> ExportUserDataAsync(string userId)
    {
        _logger.LogInformation("DecVCPlat: Starting GDPR data export for user {UserId}", userId);
        
        await _auditLogger.LogAsync("DecVCPlatGdprDataExport", userId, "Data export initiated");

        try
        {
            var userData = new DecVCPlatUserDataExport
            {
                UserId = userId,
                ExportDate = DateTime.UtcNow,
                PersonalData = await _dataRepository.GetUserPersonalDataAsync(userId),
                ProjectData = await _dataRepository.GetUserProjectDataAsync(userId),
                VotingData = await _dataRepository.GetUserVotingDataAsync(userId),
                FundingData = await _dataRepository.GetUserFundingDataAsync(userId),
                NotificationData = await _dataRepository.GetUserNotificationDataAsync(userId),
                ConsentHistory = await _dataRepository.GetConsentHistoryAsync(userId)
            };

            var exportJson = JsonSerializer.Serialize(userData, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            await _auditLogger.LogAsync("DecVCPlatGdprDataExport", userId, "Data export completed successfully");

            return new DecVCPlatDataExportResult
            {
                Success = true,
                ExportData = exportJson,
                ExportSize = System.Text.Encoding.UTF8.GetByteCount(exportJson),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to export user data for {UserId}", userId);
            await _auditLogger.LogAsync("DecVCPlatGdprDataExport", userId, $"Data export failed: {ex.Message}");
            
            return new DecVCPlatDataExportResult
            {
                Success = false,
                ErrorMessage = "Data export failed. Please contact support."
            };
        }
    }

    public async Task<bool> DeleteUserDataAsync(string userId, bool confirmDeletion = false)
    {
        if (!confirmDeletion)
        {
            _logger.LogWarning("DecVCPlat: Data deletion attempted without confirmation for user {UserId}", userId);
            return false;
        }

        _logger.LogInformation("DecVCPlat: Starting GDPR data deletion for user {UserId}", userId);
        await _auditLogger.LogAsync("DecVCPlatGdprDataDeletion", userId, "Data deletion initiated");

        try
        {
            // Delete in reverse dependency order
            await _dataRepository.DeleteUserNotificationDataAsync(userId);
            await _dataRepository.DeleteUserFundingDataAsync(userId);
            await _dataRepository.DeleteUserVotingDataAsync(userId);
            await _dataRepository.DeleteUserProjectDataAsync(userId);
            await _dataRepository.AnonymizeUserPersonalDataAsync(userId); // Anonymize instead of delete for audit trail

            await _auditLogger.LogAsync("DecVCPlatGdprDataDeletion", userId, "Data deletion completed successfully");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to delete user data for {UserId}", userId);
            await _auditLogger.LogAsync("DecVCPlatGdprDataDeletion", userId, $"Data deletion failed: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ProcessDataSubjectRequestAsync(DecVCPlatDataSubjectRequest request)
    {
        _logger.LogInformation("DecVCPlat: Processing data subject request {RequestType} for user {UserId}", 
            request.RequestType, request.UserId);

        await _auditLogger.LogAsync("DecVCPlatGdprDataSubjectRequest", request.UserId, 
            $"Data subject request received: {request.RequestType}");

        try
        {
            return request.RequestType switch
            {
                DecVCPlatDataSubjectRequestType.Export => (await ExportUserDataAsync(request.UserId)).Success,
                DecVCPlatDataSubjectRequestType.Delete => await DeleteUserDataAsync(request.UserId, request.ConfirmDeletion),
                DecVCPlatDataSubjectRequestType.Rectification => await ProcessRectificationRequestAsync(request),
                DecVCPlatDataSubjectRequestType.Restriction => await ProcessRestrictionRequestAsync(request),
                DecVCPlatDataSubjectRequestType.Portability => await ProcessPortabilityRequestAsync(request),
                _ => false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to process data subject request for {UserId}", request.UserId);
            return false;
        }
    }

    public async Task<bool> RecordConsentAsync(string userId, DecVCPlatConsentType consentType, bool granted)
    {
        _logger.LogInformation("DecVCPlat: Recording consent {ConsentType}={Granted} for user {UserId}", 
            consentType, granted, userId);

        try
        {
            var consentRecord = new DecVCPlatConsentRecord
            {
                UserId = userId,
                ConsentType = consentType,
                Granted = granted,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetCurrentIpAddress(),
                UserAgent = GetCurrentUserAgent()
            };

            await _dataRepository.RecordConsentAsync(consentRecord);
            await _auditLogger.LogAsync("DecVCPlatGdprConsent", userId, 
                $"Consent recorded: {consentType}={granted}");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to record consent for {UserId}", userId);
            return false;
        }
    }

    public async Task<DecVCPlatConsentStatus> GetConsentStatusAsync(string userId)
    {
        try
        {
            var consentRecords = await _dataRepository.GetConsentRecordsAsync(userId);
            
            return new DecVCPlatConsentStatus
            {
                UserId = userId,
                DataProcessingConsent = GetLatestConsent(consentRecords, DecVCPlatConsentType.DataProcessing),
                MarketingConsent = GetLatestConsent(consentRecords, DecVCPlatConsentType.Marketing),
                AnalyticsConsent = GetLatestConsent(consentRecords, DecVCPlatConsentType.Analytics),
                ThirdPartyConsent = GetLatestConsent(consentRecords, DecVCPlatConsentType.ThirdParty),
                LastUpdated = consentRecords.Max(c => c.Timestamp)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Failed to get consent status for {UserId}", userId);
            return new DecVCPlatConsentStatus { UserId = userId };
        }
    }

    private async Task<bool> ProcessRectificationRequestAsync(DecVCPlatDataSubjectRequest request)
    {
        // Implementation for data rectification
        await _dataRepository.UpdateUserDataAsync(request.UserId, request.RectificationData);
        return true;
    }

    private async Task<bool> ProcessRestrictionRequestAsync(DecVCPlatDataSubjectRequest request)
    {
        // Implementation for processing restriction
        await _dataRepository.RestrictUserDataProcessingAsync(request.UserId, request.RestrictionType);
        return true;
    }

    private async Task<bool> ProcessPortabilityRequestAsync(DecVCPlatDataSubjectRequest request)
    {
        // Implementation for data portability
        var exportResult = await ExportUserDataAsync(request.UserId);
        return exportResult.Success;
    }

    private bool GetLatestConsent(IEnumerable<DecVCPlatConsentRecord> records, DecVCPlatConsentType consentType)
    {
        return records
            .Where(r => r.ConsentType == consentType)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefault()?.Granted ?? false;
    }

    private string GetCurrentIpAddress()
    {
        // Implementation to get current request IP address
        return "127.0.0.1"; // Placeholder
    }

    private string GetCurrentUserAgent()
    {
        // Implementation to get current request user agent
        return "DecVCPlat-Client"; // Placeholder
    }
}

// GDPR Data Models
public class DecVCPlatDataExportResult
{
    public bool Success { get; set; }
    public string? ExportData { get; set; }
    public long ExportSize { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DecVCPlatUserDataExport
{
    public string UserId { get; set; } = string.Empty;
    public DateTime ExportDate { get; set; }
    public object? PersonalData { get; set; }
    public object? ProjectData { get; set; }
    public object? VotingData { get; set; }
    public object? FundingData { get; set; }
    public object? NotificationData { get; set; }
    public IEnumerable<DecVCPlatConsentRecord>? ConsentHistory { get; set; }
}

public class DecVCPlatDataSubjectRequest
{
    public string UserId { get; set; } = string.Empty;
    public DecVCPlatDataSubjectRequestType RequestType { get; set; }
    public bool ConfirmDeletion { get; set; }
    public object? RectificationData { get; set; }
    public string? RestrictionType { get; set; }
}

public enum DecVCPlatDataSubjectRequestType
{
    Export,
    Delete,
    Rectification,
    Restriction,
    Portability
}

public class DecVCPlatConsentRecord
{
    public string UserId { get; set; } = string.Empty;
    public DecVCPlatConsentType ConsentType { get; set; }
    public bool Granted { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public enum DecVCPlatConsentType
{
    DataProcessing,
    Marketing,
    Analytics,
    ThirdParty
}

public class DecVCPlatConsentStatus
{
    public string UserId { get; set; } = string.Empty;
    public bool DataProcessingConsent { get; set; }
    public bool MarketingConsent { get; set; }
    public bool AnalyticsConsent { get; set; }
    public bool ThirdPartyConsent { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Required interfaces (to be implemented by data layer)
public interface IDecVCPlatDataRepository
{
    Task<object> GetUserPersonalDataAsync(string userId);
    Task<object> GetUserProjectDataAsync(string userId);
    Task<object> GetUserVotingDataAsync(string userId);
    Task<object> GetUserFundingDataAsync(string userId);
    Task<object> GetUserNotificationDataAsync(string userId);
    Task<IEnumerable<DecVCPlatConsentRecord>> GetConsentHistoryAsync(string userId);
    Task DeleteUserNotificationDataAsync(string userId);
    Task DeleteUserFundingDataAsync(string userId);
    Task DeleteUserVotingDataAsync(string userId);
    Task DeleteUserProjectDataAsync(string userId);
    Task AnonymizeUserPersonalDataAsync(string userId);
    Task RecordConsentAsync(DecVCPlatConsentRecord consentRecord);
    Task<IEnumerable<DecVCPlatConsentRecord>> GetConsentRecordsAsync(string userId);
    Task UpdateUserDataAsync(string userId, object rectificationData);
    Task RestrictUserDataProcessingAsync(string userId, string restrictionType);
}

public interface IDecVCPlatAuditLogger
{
    Task LogAsync(string action, string userId, string details);
}
