using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace TapHoa.Application.Common;

public static class CacheKeys
{
    public const string CategoriesAll = "categories:all";
    public const string FlashSaleCurrent = "flashsale:current";
}

public static class CacheHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<T?> GetAsync<T>(IDistributedCache cache, string key, CancellationToken ct = default) where T : class
    {
        try
        {
            var bytes = await cache.GetAsync(key, ct);
            if (bytes is null || bytes.Length == 0) return null;
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public static async Task SetAsync<T>(IDistributedCache cache, string key, T value, TimeSpan ttl, CancellationToken ct = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            await cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, ct);
        }
        catch
        {
            // Redis unavailable — skip caching, serve from DB
        }
    }
}
