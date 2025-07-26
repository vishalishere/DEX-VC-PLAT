using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DecVCPlat.Common.Caching
{
    public static class CacheExtensions
    {
        /// <summary>
        /// Adds Redis distributed cache services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddDistributedCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Get Redis connection string from configuration
            var redisConnection = configuration.GetConnectionString("Redis");
            
            if (string.IsNullOrEmpty(redisConnection))
            {
                // If Redis connection is not configured, use in-memory cache for development
                services.AddDistributedMemoryCache();
            }
            else
            {
                // Configure Redis cache
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnection;
                    options.InstanceName = "DecVCPlat:";
                });
            }
            
            // Register our cache service
            services.AddSingleton<ICacheService, RedisCacheService>();
            
            return services;
        }
    }
}
