using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DecVCPlat.Common.Notifications.New
{
    /// <summary>
    /// Extension methods for configuring notification services
    /// </summary>
    public static class NotificationServiceExtensions
    {
        /// <summary>
        /// Adds notification services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddNotificationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register notification services here
            // This is a placeholder implementation that will be replaced with actual implementation
            services.AddSingleton<INotificationService, DefaultNotificationService>();
            
            return services;
        }
    }
}
