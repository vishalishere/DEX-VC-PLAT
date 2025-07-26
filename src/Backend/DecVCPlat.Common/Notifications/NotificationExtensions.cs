using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace DecVCPlat.Common.Notifications
{
    /// <summary>
    /// Extension methods for configuring notification services
    /// </summary>
    public static class NotificationExtensions
    {
        /// <summary>
        /// Adds notification services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register notification settings
            services.Configure<NotificationSettings>(configuration.GetSection("Notifications"));
            
            // Register MongoDB client if not already registered
            if (!services.BuildServiceProvider().GetService<IMongoClient>().IsValueCreated)
            {
                var connectionString = configuration.GetConnectionString("NotificationsDatabase") 
                    ?? "mongodb://localhost:27017";
                
                services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
            }
            
            // Register notification service
            services.AddSingleton<INotificationService, NotificationService>();
            
            return services;
        }
    }
}
