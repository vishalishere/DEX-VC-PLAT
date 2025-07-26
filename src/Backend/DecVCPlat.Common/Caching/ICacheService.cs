using System;
using System.Threading.Tasks;

namespace DecVCPlat.Common.Caching
{
    public interface ICacheService
    {
        /// <summary>
        /// Gets a cached item by key
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <returns>Cached item or null if not found</returns>
        Task<T> GetAsync<T>(string key) where T : class;
        
        /// <summary>
        /// Sets a cached item
        /// </summary>
        /// <typeparam name="T">Type of item to cache</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="value">Item to cache</param>
        /// <param name="expiration">Optional expiration timespan</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        
        /// <summary>
        /// Removes a cached item by key
        /// </summary>
        /// <param name="key">Cache key</param>
        Task RemoveAsync(string key);
        
        /// <summary>
        /// Gets an item from the cache, or if not found, creates it using the factory method
        /// </summary>
        /// <typeparam name="T">Type of cached item</typeparam>
        /// <param name="key">Cache key</param>
        /// <param name="factory">Factory method to create the item if not found</param>
        /// <param name="expiration">Optional expiration timespan</param>
        /// <returns>Cached item or newly created item</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
    }
}
