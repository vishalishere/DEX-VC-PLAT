using System;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DecVCPlat.Common.Monitoring
{
    public static class MonitoringExtensions
    {
        /// <summary>
        /// Adds application monitoring services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <param name="serviceName">Name of the service being monitored</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMonitoringServices(
            this IServiceCollection services, 
            IConfiguration configuration, 
            string serviceName)
        {
            // Configure Application Insights if instrumentation key is provided
            var appInsightsKey = configuration["ApplicationInsights:InstrumentationKey"];
            
            if (!string.IsNullOrEmpty(appInsightsKey))
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.InstrumentationKey = appInsightsKey;
                });
                
                // Add custom telemetry initializer to add service name to all telemetry
                services.AddSingleton<ITelemetryInitializer>(new ServiceNameTelemetryInitializer(serviceName));
                
                // Enable SQL dependency tracking
                services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
                {
                    module.EnableSqlCommandTextInstrumentation = true;
                });
            }
            
            // Add health checks
            services.AddHealthChecks()
                .AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    name: "database",
                    tags: new[] { "db", "sql", "sqlserver" });
            
            // Add memory health check
            services.AddHealthChecks()
                .AddProcessAllocatedMemoryHealthCheck(
                    maximumMegabytesAllocated: 1024,
                    name: "process-memory",
                    tags: new[] { "memory" });
            
            // Configure logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddDebug();
                
                // Add Application Insights logging if configured
                if (!string.IsNullOrEmpty(appInsightsKey))
                {
                    builder.AddApplicationInsights(appInsightsKey);
                }
            });
            
            // Register performance monitoring service
            services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
            
            return services;
        }
    }
}
