using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace TapHoa.Application.Common;

public static class CacheKeys
{
    public const string CategoriesAll = "categories:all";
    public const string FlashSaleCurrent = "flashsale:current";
}

public interface ICacheHelper
{
    Task<T?> GetAsync<T>(IDistributedCache cache, string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(IDistributedCache cache, string key, T value, TimeSpan ttl, CancellationToken ct = default);
}

public class CacheHelper(ILogger<CacheHelper> logger) : ICacheHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<T?> GetAsync<T>(IDistributedCache cache, string key, CancellationToken ct = default) where T : class
    {
        try
        {
            var bytes = await cache.GetAsync(key, ct);
            if (bytes is null || bytes.Length == 0) return null;
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache read failed for key {CacheKey}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(IDistributedCache cache, string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache write failed for key {CacheKey}", key);
        }
    }
}
