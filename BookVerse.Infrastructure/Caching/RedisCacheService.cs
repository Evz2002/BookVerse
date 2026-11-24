namespace BookVerse.Infrastructure.Caching;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache distributedCache,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedJson = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrWhiteSpace(cachedJson))
            {
                _logger.LogInformation("Redis Cache MISS for key {CacheKey}", key);
                return default;
            }

            _logger.LogInformation("Redis Cache HIT for key {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cachedJson);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable while reading key {CacheKey}. Falling back to database.", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            var json = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, json, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable while writing key {CacheKey}.", key);
        }
    }
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogInformation("Redis Cache Invalidated for key {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis unavailable while removing key {CacheKey}.", key);
        }
    }
}