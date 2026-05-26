using Microsoft.EntityFrameworkCore;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.FlashSale;

public static class FlashSaleEndpoints
{
    public static void MapFlashSaleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/flash-sale").WithTags("Flash Sale");

        group.MapGet("/current", async (AppDbContext db) =>
        {
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

            return Results.Ok(new
            {
                sessionId = session.Id,
                name      = session.Name,
                startTime = session.StartTime,
                endTime   = session.EndTime,
                products  = session.Items
                    .Where(i => i.Product.IsActive)
                    .Select(i => new
                    {
                        id             = i.ProductId,
                        name           = i.Product.Name,
                        thumbnailUrl   = i.Product.ThumbnailUrl,
                        categoryName   = i.Product.Category.Name,
                        originalPrice  = i.Product.Price,
                        flashSalePrice = i.FlashSalePrice,
                        flashSaleStock = i.FlashSaleStock,
                        soldCount      = i.SoldCount,
                        stockRemaining = i.FlashSaleStock - i.SoldCount,
                    })
                    .ToList(),
            });
        });
    }
}
