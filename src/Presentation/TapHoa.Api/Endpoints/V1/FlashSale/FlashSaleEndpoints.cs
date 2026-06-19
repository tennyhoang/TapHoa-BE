using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Application.Common;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.FlashSale;

public static class FlashSaleEndpoints
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(15);

    public static void MapFlashSaleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/flash-sale").WithTags("Flash Sale");

        group.MapGet("/current", async (AppDbContext db, IDistributedCache cache, ICacheHelper cacheHelper) =>
        {
            var cached = await cacheHelper.GetAsync<FlashSaleCurrentResponse>(cache, CacheKeys.FlashSaleCurrent);
            if (cached is not null)
                return Results.Ok(cached);

            var now = DateTime.UtcNow;

            var session = await db.FlashSaleSessions
                .Where(s => s.IsActive && s.StartTime <= now && s.EndTime > now)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();

            if (session is null)
                return Results.Ok((object?)null);

            var result = new FlashSaleCurrentResponse(
                SessionId: session.Id,
                Name: session.Name,
                StartTime: session.StartTime,
                EndTime: session.EndTime,
                Products: session.Items
                    .Where(i => i.Product.IsActive)
                    .Select(i => new FlashSaleProductResponse(
                        Id: i.ProductId,
                        Name: i.Product.Name,
                        ThumbnailUrl: i.Product.ThumbnailUrl,
                        CategoryName: i.Product.Category != null ? i.Product.Category.Name : "",
                        OriginalPrice: i.Product.Price,
                        FlashSalePrice: i.FlashSalePrice,
                        FlashSaleStock: i.FlashSaleStock,
                        SoldCount: i.SoldCount,
                        StockRemaining: i.FlashSaleStock - i.SoldCount
                    ))
                    .ToList()
            );

            await cacheHelper.SetAsync(cache, CacheKeys.FlashSaleCurrent, result, CacheTtl);
            return Results.Ok(result);
        });
    }
}
