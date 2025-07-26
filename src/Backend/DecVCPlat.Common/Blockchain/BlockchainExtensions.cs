using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DecVCPlat.Common.Blockchain
{
    /// <summary>
    /// Extension methods for configuring blockchain services
    /// </summary>
    public static class BlockchainExtensions
    {
        /// <summary>
        /// Adds blockchain services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBlockchainServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register blockchain settings
            services.Configure<BlockchainSettings>(configuration.GetSection("Blockchain"));
            
            // Register blockchain service
            services.AddSingleton<IBlockchainService, EthereumBlockchainService>();
            
            return services;
        }
    }
}
