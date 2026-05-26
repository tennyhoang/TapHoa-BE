using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminFlashSaleEndpoints
{
    public static void MapAdminFlashSaleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/flash-sale")
            .WithTags("Admin - Flash Sale")
            .RequireAuthorization("Admin");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var sessions = await db.FlashSaleSessions
                .Include(s => s.Items)
                .OrderByDescending(s => s.StartTime)
                .Select(s => new
                {
                    s.Id, s.Name, s.StartTime, s.EndTime, s.IsActive, s.CreatedAt,
                    itemCount = s.Items.Count,
                })
                .ToListAsync();

            return Results.Ok(sessions);
        });

        group.MapPost("/", async ([FromBody] CreateFlashSaleSessionRequest req, AppDbContext db) =>
        {
            var session = new FlashSaleSession
            {
                Name      = req.Name,
                StartTime = req.StartTime.ToUniversalTime(),
                EndTime   = req.EndTime.ToUniversalTime(),
                IsActive  = req.IsActive,
            };

            db.FlashSaleSessions.Add(session);
            await db.SaveChangesAsync();

            return Results.Ok(new { session.Id });
        });

        group.MapPatch("/{id:guid}/toggle", async (Guid id, AppDbContext db) =>
        {
            var session = await db.FlashSaleSessions.FindAsync(id);
            if (session is null) return Results.NotFound();

            session.IsActive = !session.IsActive;
            await db.SaveChangesAsync();

            return Results.Ok(new { session.IsActive });
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var session = await db.FlashSaleSessions.FindAsync(id);
            if (session is null) return Results.NotFound();

            db.FlashSaleSessions.Remove(session);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        group.MapGet("/{id:guid}/items", async (Guid id, AppDbContext db) =>
        {
            var items = await db.FlashSaleItems
                .Where(i => i.FlashSaleSessionId == id)
                .Include(i => i.Product)
                .Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    productName    = i.Product.Name,
                    thumbnailUrl   = i.Product.ThumbnailUrl,
                    originalPrice  = i.Product.Price,
                    i.FlashSalePrice,
                    i.FlashSaleStock,
                    i.SoldCount,
                })
                .ToListAsync();

            return Results.Ok(items);
        });

        group.MapPost("/{id:guid}/items", async (
            Guid id,
            [FromBody] AddFlashSaleItemRequest req,
            AppDbContext db) =>
        {
            var sessionExists = await db.FlashSaleSessions.AnyAsync(s => s.Id == id);
            if (!sessionExists) return Results.NotFound();

            var productExists = await db.Products.AnyAsync(p => p.Id == req.ProductId && p.IsActive);
            if (!productExists)
                return Results.BadRequest(new { error = "Sản phẩm không tồn tại hoặc đã bị ẩn" });

            var alreadyAdded = await db.FlashSaleItems
                .AnyAsync(i => i.FlashSaleSessionId == id && i.ProductId == req.ProductId);
            if (alreadyAdded)
                return Results.BadRequest(new { error = "Sản phẩm đã được thêm vào phiên này" });

            var item = new FlashSaleItem
            {
                FlashSaleSessionId = id,
                ProductId          = req.ProductId,
                FlashSalePrice     = req.FlashSalePrice,
                FlashSaleStock     = req.FlashSaleStock,
                SoldCount          = 0,
            };

            db.FlashSaleItems.Add(item);
            await db.SaveChangesAsync();

            return Results.Ok(new { item.Id });
        });

        group.MapDelete("/{id:guid}/items/{itemId:guid}", async (Guid id, Guid itemId, AppDbContext db) =>
        {
            var item = await db.FlashSaleItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.FlashSaleSessionId == id);

            if (item is null) return Results.NotFound();

            db.FlashSaleItems.Remove(item);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}

public record CreateFlashSaleSessionRequest(string Name, DateTime StartTime, DateTime EndTime, bool IsActive = true);
public record AddFlashSaleItemRequest(Guid ProductId, decimal FlashSalePrice, int FlashSaleStock);
