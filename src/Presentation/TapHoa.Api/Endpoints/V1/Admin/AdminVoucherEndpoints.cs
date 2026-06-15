using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminVoucherEndpoints
{
    public static void MapAdminVoucherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/vouchers")
            .WithTags("Admin - Vouchers")
            .RequireAuthorization("Admin");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var vouchers = await db.Vouchers
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new
                {
                    v.Id, v.Code, v.Type, v.DiscountValue, v.MinOrderAmount,
                    v.UsageLimit, v.UsedCount, v.IsActive, v.ExpiresAt,
                    v.MaxDiscountAmount, v.ExcludeFlashSaleItems, v.CreatedAt,
                })
                .ToListAsync();

            return Results.Ok(vouchers);
        });

        group.MapPost("/", async ([FromBody] CreateVoucherRequest req, AppDbContext db) =>
        {
            var codeExists = await db.Vouchers
                .AnyAsync(v => v.Code.ToUpper() == req.Code.ToUpper());
            if (codeExists)
                return Results.BadRequest(new { error = "Mã voucher đã tồn tại" });

            var voucher = new Voucher
            {
                Code                 = req.Code.ToUpper().Trim(),
                Type                 = req.Type,
                DiscountValue        = req.DiscountValue,
                MinOrderAmount       = req.MinOrderAmount,
                UsageLimit           = req.UsageLimit,
                IsActive             = req.IsActive,
                ExpiresAt            = req.ExpiresAt?.ToUniversalTime(),
                MaxDiscountAmount    = req.MaxDiscountAmount,
                ExcludeFlashSaleItems = req.ExcludeFlashSaleItems,
            };

            db.Vouchers.Add(voucher);
            await db.SaveChangesAsync();

            return Results.Ok(new { voucher.Id });
        });

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateVoucherRequest req, AppDbContext db) =>
        {
            var voucher = await db.Vouchers.FindAsync(id);
            if (voucher is null) return Results.NotFound();

            var codeExists = await db.Vouchers
                .AnyAsync(v => v.Code.ToUpper() == req.Code.ToUpper() && v.Id != id);
            if (codeExists)
                return Results.BadRequest(new { error = "Mã voucher đã tồn tại" });

            voucher.Code                  = req.Code.ToUpper().Trim();
            voucher.Type                  = req.Type;
            voucher.DiscountValue         = req.DiscountValue;
            voucher.MinOrderAmount        = req.MinOrderAmount;
            voucher.UsageLimit            = req.UsageLimit;
            voucher.IsActive              = req.IsActive;
            voucher.ExpiresAt             = req.ExpiresAt?.ToUniversalTime();
            voucher.MaxDiscountAmount     = req.MaxDiscountAmount;
            voucher.ExcludeFlashSaleItems = req.ExcludeFlashSaleItems;

            await db.SaveChangesAsync();
            return Results.Ok();
        });

        group.MapPatch("/{id:guid}/toggle", async (Guid id, AppDbContext db) =>
        {
            var voucher = await db.Vouchers.FindAsync(id);
            if (voucher is null) return Results.NotFound();

            voucher.IsActive = !voucher.IsActive;
            await db.SaveChangesAsync();

            return Results.Ok(new { voucher.IsActive });
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var voucher = await db.Vouchers.FindAsync(id);
            if (voucher is null) return Results.NotFound();

            db.Vouchers.Remove(voucher);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });
    }
}

public record CreateVoucherRequest(
    string Code,
    string Type,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    int? UsageLimit,
    bool IsActive,
    DateTime? ExpiresAt,
    decimal? MaxDiscountAmount,
    bool ExcludeFlashSaleItems = true);

public record UpdateVoucherRequest(
    string Code,
    string Type,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    int? UsageLimit,
    bool IsActive,
    DateTime? ExpiresAt,
    decimal? MaxDiscountAmount,
    bool ExcludeFlashSaleItems = true);
