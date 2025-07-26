using System;
using System.Threading.Tasks;

namespace DecVCPlat.Common.Monitoring
{
    /// <summary>
    /// Interface for performance monitoring service
    /// </summary>
    public interface IPerformanceMonitoringService
    {
        /// <summary>
        /// Tracks the execution time of a synchronous operation
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to track</param>
        /// <param name="operationName">Name of the operation for tracking</param>
        /// <param name="properties">Optional properties to include with the telemetry</param>
        /// <returns>The result of the operation</returns>
        T TrackExecutionTime<T>(Func<T> operation, string operationName, params (string Key, string Value)[] properties);

        /// <summary>
        /// Tracks the execution time of an asynchronous operation
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The asynchronous operation to track</param>
        /// <param name="operationName">Name of the operation for tracking</param>
        /// <param name="properties">Optional properties to include with the telemetry</param>
        /// <returns>The result of the operation</returns>
        Task<T> TrackExecutionTimeAsync<T>(Func<Task<T>> operation, string operationName, params (string Key, string Value)[] properties);

        /// <summary>
        /// Tracks the execution time of an asynchronous operation that returns no result
        /// </summary>
        /// <param name="operation">The asynchronous operation to track</param>
        /// <param name="operationName">Name of the operation for tracking</param>
        /// <param name="properties">Optional properties to include with the telemetry</param>
        Task TrackExecutionTimeAsync(Func<Task> operation, string operationName, params (string Key, string Value)[] properties);

        /// <summary>
        /// Tracks a dependency call (e.g., database, external API)
        /// </summary>
        /// <typeparam name="T">Return type of the operation</typeparam>
        /// <param name="operation">The operation to track</param>
        /// <param name="dependencyType">Type of dependency (e.g., SQL, HTTP)</param>
        /// <param name="dependencyName">Name of the dependency</param>
        /// <param name="commandText">Command or query text if applicable</param>
        /// <returns>The result of the operation</returns>
        Task<T> TrackDependencyAsync<T>(Func<Task<T>> operation, string dependencyType, string dependencyName, string commandText = null);

        /// <summary>
        /// Tracks an exception
        /// </summary>
        /// <param name="exception">The exception to track</param>
        /// <param name="properties">Optional properties to include with the telemetry</param>
        void TrackException(Exception exception, params (string Key, string Value)[] properties);
    }
}
