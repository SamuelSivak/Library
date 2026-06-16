using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Library.Services
{
    public class HybridCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDistributedCache _redisCache;
        private readonly ILogger<HybridCacheService> _logger;

        public HybridCacheService(IMemoryCache memoryCache, IDistributedCache redisCache, ILogger<HybridCacheService> logger)
        {
            _memoryCache = memoryCache;
            _redisCache = redisCache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            
            if (_memoryCache.TryGetValue(key, out T? localValue))
            {
                _logger.LogInformation("L1 Cache Hit for key: {Key}", key);
                return localValue;
            }

            _logger.LogInformation("L1 Cache Miss for key: {Key}. Checking L2 Redis Cache...", key);

            
            try
            {
                var redisData = await _redisCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(redisData))
                {
                    var value = JsonSerializer.Deserialize<T>(redisData);
                    if (value != null)
                    {
                        _logger.LogInformation("L2 Cache Hit for key: {Key}. Saving to L1...", key);
                        
                        _memoryCache.Set(key, value, TimeSpan.FromMinutes(2));
                        return value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading from L2 Redis for key {Key}", key);
            }

            _logger.LogInformation("L2 Cache Miss for key: {Key}. Fetching from Database...", key);
            return default;
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expirationL2 = null)
        {
            
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(2));

            
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expirationL2 ?? TimeSpan.FromMinutes(30)
                };
                var serialized = JsonSerializer.Serialize(value);
                await _redisCache.SetStringAsync(key, serialized, options);
                _logger.LogInformation("Saved key {Key} to L1 (Memory) and L2 (Redis)", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving key {Key} to L2 Redis", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            
            _memoryCache.Remove(key);

            
            try
            {
                await _redisCache.RemoveAsync(key);
                _logger.LogInformation("Removed key {Key} from L1 (Memory) and L2 (Redis)", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing key {Key} from L2 Redis", key);
            }
        }
    }
}
