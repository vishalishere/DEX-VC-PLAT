// © 2024 DecVCPlat. All rights reserved.

namespace DecVCPlat.Shared.Services;

public interface IDecVCPlatMetricsRepository
{
    Task<int> GetAuthenticationAttemptsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetFailedLoginAttemptsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetSecurityValidationsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetSecurityDenialsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetSecurityChallengesAsync(DateTime startTime, DateTime endTime);
    Task<int> GetThreatDetectionsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetBlockedRequestsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetDataExportRequestsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetDataDeletionRequestsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetConsentUpdatesAsync(DateTime startTime, DateTime endTime);
    Task<int> GetNewUserRegistrationsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetActiveUsersAsync(DateTime startTime, DateTime endTime);
    Task<int> GetNewProjectsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetProjectsApprovedAsync(DateTime startTime, DateTime endTime);
    Task<int> GetProjectsRejectedAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetTotalFundingRaisedAsync(DateTime startTime, DateTime endTime);
    Task<int> GetSuccessfulFundingRoundsAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageFundingAmountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalVotesAsync(DateTime startTime, DateTime endTime);
    Task<int> GetBlockchainTransactionsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTokenStakingEventsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalApiRequestsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetSuccessfulApiRequestsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetFailedApiRequestsAsync(DateTime startTime, DateTime endTime);
    Task<double> GetAverageResponseTimeAsync(DateTime startTime, DateTime endTime);
    Task<double> GetRequestsPerSecondAsync(DateTime startTime, DateTime endTime);
    Task<double> GetAverageQueryTimeAsync(DateTime startTime, DateTime endTime);
    Task<int> GetSlowQueriesCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetDatabaseConnectionCountAsync(DateTime startTime, DateTime endTime);
    Task<int> GetDeadlockCountAsync(DateTime startTime, DateTime endTime);
    Task<double> GetAverageCpuUsageAsync(DateTime startTime, DateTime endTime);
    Task<double> GetAverageMemoryUsageAsync(DateTime startTime, DateTime endTime);
    Task<double> GetAverageDiskUsageAsync(DateTime startTime, DateTime endTime);
    Task<double> GetNetworkThroughputAsync(DateTime startTime, DateTime endTime);
    Task<int> GetPageViewsAsync(DateTime startTime, DateTime endTime);
    Task<TimeSpan> GetAverageSessionDurationAsync(DateTime startTime, DateTime endTime);
    Task<double> GetBounceRateAsync(DateTime startTime, DateTime endTime);
    Task<long> GetTotalGasUsedAsync(DateTime startTime, DateTime endTime);
    Task<decimal> GetAverageGasPriceAsync(DateTime startTime, DateTime endTime);
    Task<int> GetFailedTransactionsAsync(DateTime startTime, DateTime endTime);
    Task<int> GetTotalUsersAsync(DateTime startTime);
    Task<int> GetReturningUsersAsync(DateTime startTime, DateTime endTime);
    Task<int> GetEligibleVotersAsync(DateTime startTime, DateTime endTime);
    Task<int> GetActualVotersAsync(DateTime startTime, DateTime endTime);
    Task RecordCustomEventAsync(string eventName, Dictionary<string, string> properties, DateTime timestamp);
    Task RecordCustomMetricAsync(string metricName, double value, Dictionary<string, string> properties, DateTime timestamp);
}

public interface IDecVCPlatHealthCheckService
{
    Task<DecVCPlatServiceHealth> CheckServiceHealthAsync(string serviceName);
    Task<DecVCPlatDatabaseHealth> CheckDatabaseHealthAsync();
}

public class DecVCPlatServiceHealth
{
    public DecVCPlatHealthStatus Status { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastChecked { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DecVCPlatDatabaseHealth
{
    public DecVCPlatHealthStatus Status { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime LastChecked { get; set; }
    public int ConnectionCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum DecVCPlatHealthStatus { Healthy, Degraded, Unhealthy }
