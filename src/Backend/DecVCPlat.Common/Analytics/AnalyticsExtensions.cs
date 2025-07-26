using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DecVCPlat.Common.Analytics
{
    /// <summary>
    /// Extension methods for configuring analytics services
    /// </summary>
    public static class AnalyticsExtensions
    {
        /// <summary>
        /// Adds analytics services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddAnalyticsServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register analytics settings
            services.Configure<AnalyticsSettings>(configuration.GetSection("Analytics"));
            
            // Register MongoDB client if not already registered
            if (!services.BuildServiceProvider().GetService<IMongoClient>().IsNotNull())
            {
                var connectionString = configuration.GetConnectionString("AnalyticsDatabase") 
                    ?? "mongodb://localhost:27017";
                
                services.AddSingleton<IMongoClient>(new MongoClient(connectionString));
            }
            
            // Register analytics service
            services.AddSingleton<IAnalyticsService, AnalyticsService>();
            
            return services;
        }
    }
}
