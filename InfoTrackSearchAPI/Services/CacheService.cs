using InfoTrackSearchAPI.Interfaces;
using InfoTrackSearchAPI.Settings;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace InfoTrackSearchAPI.Services;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly CacheSettings _cacheSettings;


    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger, IOptions<CacheSettings> cacheSettings)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheSettings = cacheSettings?.Value ?? throw new ArgumentNullException(nameof(cacheSettings));

    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> createItem)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
        }

        if (!_memoryCache.TryGetValue(key, out T cacheEntry))
        {
            _logger.LogInformation($"Cache miss for key: {key}. Creating new cache entry.");
            try
            {
                cacheEntry = await createItem();
                _memoryCache.Set(key, cacheEntry, TimeSpan.FromMinutes(_cacheSettings.ExpirationMinutes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating cache entry for key: {key}");
                throw;
            }
        }
        else
        {
            _logger.LogInformation($"Cache hit for key: {key}");
        }

        return cacheEntry;
    }
}