using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

namespace DecVCPlat.Common.Monitoring
{
    /// <summary>
    /// Service for monitoring performance of operations
    /// </summary>
    public class PerformanceMonitoringService : IPerformanceMonitoringService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<PerformanceMonitoringService> _logger;
        private readonly bool _isApplicationInsightsEnabled;

        public PerformanceMonitoringService(
            TelemetryConfiguration telemetryConfiguration,
            ILogger<PerformanceMonitoringService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Check if Application Insights is configured
            _isApplicationInsightsEnabled = !string.IsNullOrEmpty(telemetryConfiguration?.InstrumentationKey);
            
            if (_isApplicationInsightsEnabled)
            {
                _telemetryClient = new TelemetryClient(telemetryConfiguration);
            }
        }

        /// <inheritdoc />
        public T TrackExecutionTime<T>(Func<T> operation, string operationName, params (string Key, string Value)[] properties)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = operation();
                stopwatch.Stop();
                
                TrackMetric(operationName, stopwatch.ElapsedMilliseconds, properties);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TrackException(ex, properties);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<T> TrackExecutionTimeAsync<T>(Func<Task<T>> operation, string operationName, params (string Key, string Value)[] properties)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var result = await operation();
                stopwatch.Stop();
                
                TrackMetric(operationName, stopwatch.ElapsedMilliseconds, properties);
                
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TrackException(ex, properties);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task TrackExecutionTimeAsync(Func<Task> operation, string operationName, params (string Key, string Value)[] properties)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                await operation();
                stopwatch.Stop();
                
                TrackMetric(operationName, stopwatch.ElapsedMilliseconds, properties);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                TrackException(ex, properties);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<T> TrackDependencyAsync<T>(Func<Task<T>> operation, string dependencyType, string dependencyName, string commandText = null)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            var stopwatch = Stopwatch.StartNew();
            bool success = false;
            
            try
            {
                var result = await operation();
                success = true;
                return result;
            }
            catch (Exception ex)
            {
                TrackException(ex, ("DependencyType", dependencyType), ("DependencyName", dependencyName));
                throw;
            }
            finally
            {
                stopwatch.Stop();
                
                if (_isApplicationInsightsEnabled)
                {
                    var dependency = new DependencyTelemetry
                    {
                        Type = dependencyType,
                        Name = dependencyName,
                        Data = commandText,
                        Duration = stopwatch.Elapsed,
                        Success = success,
                        Timestamp = DateTimeOffset.UtcNow
                    };
                    
                    _telemetryClient.TrackDependency(dependency);
                }
                
                _logger.LogInformation(
                    "Dependency {DependencyType} - {DependencyName} completed in {ElapsedMilliseconds}ms with success: {Success}",
                    dependencyType,
                    dependencyName,
                    stopwatch.ElapsedMilliseconds,
                    success);
            }
        }

        /// <inheritdoc />
        public void TrackException(Exception exception, params (string Key, string Value)[] properties)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (_isApplicationInsightsEnabled)
            {
                var telemetryProperties = ConvertToProperties(properties);
                _telemetryClient.TrackException(exception, telemetryProperties);
            }
            
            // Always log the exception
            _logger.LogError(exception, "Exception occurred: {ExceptionMessage}", exception.Message);
        }

        private void TrackMetric(string metricName, long elapsedMilliseconds, params (string Key, string Value)[] properties)
        {
            if (_isApplicationInsightsEnabled)
            {
                var telemetryProperties = ConvertToProperties(properties);
                _telemetryClient.TrackMetric(metricName, elapsedMilliseconds, telemetryProperties);
            }
            
            _logger.LogInformation("{MetricName} completed in {ElapsedMilliseconds}ms", metricName, elapsedMilliseconds);
        }

        private Dictionary<string, string> ConvertToProperties(params (string Key, string Value)[] properties)
        {
            var dictionary = new Dictionary<string, string>();
            
            if (properties != null)
            {
                foreach (var (key, value) in properties)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        dictionary[key] = value;
                    }
                }
            }
            
            return dictionary;
        }
    }
}
