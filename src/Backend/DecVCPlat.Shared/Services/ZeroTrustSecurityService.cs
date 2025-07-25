// © 2024 DecVCPlat. All rights reserved.

using Microsoft.Extensions.Logging;

namespace DecVCPlat.Shared.Services;

public interface IDecVCPlatZeroTrustSecurityService
{
    Task<DecVCPlatSecurityContext> ValidateRequestSecurityAsync(DecVCPlatSecurityRequest request);
    Task<bool> AuthorizeResourceAccessAsync(string userId, string resourceId, string action);
    Task<DecVCPlatThreatAssessment> AssessThreatLevelAsync(DecVCPlatSecurityRequest request);
    Task<bool> EnforceNetworkSecurityPolicyAsync(string sourceIp, string targetResource);
    Task<bool> ValidateDeviceComplianceAsync(string deviceFingerprint, string userId);
    Task<DecVCPlatRiskScore> CalculateRiskScoreAsync(string userId, DecVCPlatSecurityRequest request);
}

public class DecVCPlatZeroTrustSecurityService : IDecVCPlatZeroTrustSecurityService
{
    private readonly ILogger<DecVCPlatZeroTrustSecurityService> _logger;
    private readonly IDecVCPlatSecurityRepository _securityRepository;
    private readonly IDecVCPlatAuditLogger _auditLogger;

    public DecVCPlatZeroTrustSecurityService(
        ILogger<DecVCPlatZeroTrustSecurityService> logger,
        IDecVCPlatSecurityRepository securityRepository,
        IDecVCPlatAuditLogger auditLogger)
    {
        _logger = logger;
        _securityRepository = securityRepository;
        _auditLogger = auditLogger;
    }

    public async Task<DecVCPlatSecurityContext> ValidateRequestSecurityAsync(DecVCPlatSecurityRequest request)
    {
        _logger.LogInformation("DecVCPlat: Zero Trust security validation for {UserId}", request.UserId);

        var context = new DecVCPlatSecurityContext
        {
            RequestId = request.RequestId,
            UserId = request.UserId,
            ValidatedAt = DateTime.UtcNow,
            IdentityVerified = await ValidateIdentityAsync(request.UserId),
            DeviceCompliant = await ValidateDeviceComplianceAsync(request.DeviceFingerprint, request.UserId),
            NetworkSecurityPassed = await EnforceNetworkSecurityPolicyAsync(request.SourceIP, request.ResourceId),
            AccessAuthorized = await AuthorizeResourceAccessAsync(request.UserId, request.ResourceId, request.Action),
            RiskScore = await CalculateRiskScoreAsync(request.UserId, request)
        };

        context.SecurityDecision = DetermineSecurityDecision(context);
        
        await _auditLogger.LogAsync("DecVCPlatZeroTrust", request.UserId, 
            $"Security validation: {context.SecurityDecision}");

        return context;
    }

    public async Task<bool> AuthorizeResourceAccessAsync(string userId, string resourceId, string action)
    {
        try
        {
            var userRoles = await _securityRepository.GetUserRolesAsync(userId);
            var resourcePermissions = await _securityRepository.GetResourcePermissionsAsync(resourceId);
            
            var authorized = resourcePermissions.Any(p => 
                userRoles.Contains(p.Role) && p.Actions.Contains(action));

            await _auditLogger.LogAsync("DecVCPlatAuthorization", userId, 
                $"Access {(authorized ? "granted" : "denied")}: {resourceId}#{action}");

            return authorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DecVCPlat: Authorization failed for {UserId}", userId);
            return false;
        }
    }

    public async Task<DecVCPlatThreatAssessment> AssessThreatLevelAsync(DecVCPlatSecurityRequest request)
    {
        return new DecVCPlatThreatAssessment
        {
            RequestId = request.RequestId,
            AssessedAt = DateTime.UtcNow,
            IpThreatLevel = await CheckIpThreatAsync(request.SourceIP),
            BehaviorThreatLevel = await CheckBehaviorThreatAsync(request.UserId),
            DeviceThreatLevel = await CheckDeviceThreatAsync(request.DeviceFingerprint),
            OverallThreatLevel = DecVCPlatThreatLevel.Low
        };
    }

    public async Task<bool> EnforceNetworkSecurityPolicyAsync(string sourceIp, string targetResource)
    {
        return await CheckIpPolicyAsync(sourceIp) && await CheckRateLimitAsync(sourceIp);
    }

    public async Task<bool> ValidateDeviceComplianceAsync(string deviceFingerprint, string userId)
    {
        var deviceInfo = await _securityRepository.GetDeviceInfoAsync(deviceFingerprint);
        return deviceInfo?.Status == DecVCPlatDeviceStatus.Trusted;
    }

    public async Task<DecVCPlatRiskScore> CalculateRiskScoreAsync(string userId, DecVCPlatSecurityRequest request)
    {
        var overallScore = (await CalculateUserRiskAsync(userId) + 
                           await CalculateBehaviorRiskAsync(request) + 
                           await CalculateEnvironmentRiskAsync(request)) / 3.0;

        return new DecVCPlatRiskScore
        {
            UserId = userId,
            OverallRiskScore = overallScore,
            RiskLevel = overallScore > 0.7 ? DecVCPlatRiskLevel.High : 
                       overallScore > 0.4 ? DecVCPlatRiskLevel.Medium : DecVCPlatRiskLevel.Low
        };
    }

    // Private helper methods
    private async Task<bool> ValidateIdentityAsync(string userId)
    {
        var userInfo = await _securityRepository.GetUserSecurityInfoAsync(userId);
        return userInfo?.IsActive == true;
    }

    private DecVCPlatSecurityDecision DetermineSecurityDecision(DecVCPlatSecurityContext context)
    {
        if (!context.IdentityVerified || !context.DeviceCompliant || !context.NetworkSecurityPassed)
            return DecVCPlatSecurityDecision.Deny;
        
        if (context.RiskScore.OverallRiskScore > 0.7)
            return DecVCPlatSecurityDecision.Challenge;
        
        return context.AccessAuthorized ? DecVCPlatSecurityDecision.Allow : DecVCPlatSecurityDecision.Deny;
    }

    private async Task<DecVCPlatThreatLevel> CheckIpThreatAsync(string sourceIp) => DecVCPlatThreatLevel.Low;
    private async Task<DecVCPlatThreatLevel> CheckBehaviorThreatAsync(string userId) => DecVCPlatThreatLevel.Low;
    private async Task<DecVCPlatThreatLevel> CheckDeviceThreatAsync(string deviceFingerprint) => DecVCPlatThreatLevel.Low;
    private async Task<bool> CheckIpPolicyAsync(string sourceIp) => true;
    private async Task<bool> CheckRateLimitAsync(string sourceIp) => true;
    private async Task<double> CalculateUserRiskAsync(string userId) => 0.2;
    private async Task<double> CalculateBehaviorRiskAsync(DecVCPlatSecurityRequest request) => 0.1;
    private async Task<double> CalculateEnvironmentRiskAsync(DecVCPlatSecurityRequest request) => 0.15;
}

// Data Models
public class DecVCPlatSecurityRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string SourceIP { get; set; } = string.Empty;
    public string DeviceFingerprint { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class DecVCPlatSecurityContext
{
    public string RequestId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ValidatedAt { get; set; }
    public bool IdentityVerified { get; set; }
    public bool DeviceCompliant { get; set; }
    public bool NetworkSecurityPassed { get; set; }
    public bool AccessAuthorized { get; set; }
    public DecVCPlatRiskScore RiskScore { get; set; } = new();
    public DecVCPlatSecurityDecision SecurityDecision { get; set; }
}

public class DecVCPlatThreatAssessment
{
    public string RequestId { get; set; } = string.Empty;
    public DateTime AssessedAt { get; set; }
    public DecVCPlatThreatLevel IpThreatLevel { get; set; }
    public DecVCPlatThreatLevel BehaviorThreatLevel { get; set; }
    public DecVCPlatThreatLevel DeviceThreatLevel { get; set; }
    public DecVCPlatThreatLevel OverallThreatLevel { get; set; }
}

public class DecVCPlatRiskScore
{
    public string UserId { get; set; } = string.Empty;
    public double OverallRiskScore { get; set; }
    public DecVCPlatRiskLevel RiskLevel { get; set; }
}

public enum DecVCPlatSecurityDecision { Allow, Deny, Challenge }
public enum DecVCPlatThreatLevel { Low, Medium, High, Critical }
public enum DecVCPlatRiskLevel { Minimal, Low, Medium, High }
public enum DecVCPlatDeviceStatus { Unknown, Trusted, Suspicious, Blocked }

// Required interfaces
public interface IDecVCPlatSecurityRepository
{
    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<IEnumerable<DecVCPlatResourcePermission>> GetResourcePermissionsAsync(string resourceId);
    Task<DecVCPlatUserSecurityInfo?> GetUserSecurityInfoAsync(string userId);
    Task<DecVCPlatDeviceInfo?> GetDeviceInfoAsync(string deviceFingerprint);
}

public class DecVCPlatResourcePermission
{
    public string Role { get; set; } = string.Empty;
    public List<string> Actions { get; set; } = new();
}

public class DecVCPlatUserSecurityInfo
{
    public bool IsActive { get; set; }
    public DecVCPlatTrustLevel TrustLevel { get; set; }
}

public class DecVCPlatDeviceInfo
{
    public DecVCPlatDeviceStatus Status { get; set; }
    public DecVCPlatTrustLevel TrustLevel { get; set; }
}

public enum DecVCPlatTrustLevel { Low, Medium, High }
