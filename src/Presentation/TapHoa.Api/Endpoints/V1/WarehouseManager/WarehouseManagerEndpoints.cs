using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.WarehouseManager;

public static class WarehouseManagerEndpoints
{
    public static void MapWarehouseManagerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/warehouse-manager")
            .WithTags("WarehouseManager")
            .RequireAuthorization("WarehouseManager");

        // GET /warehouse-manager/dashboard — tổng quan kho
        group.MapGet("/dashboard", async (ClaimsPrincipal user, AppDbContext db) =>
        {
            var managerId = GetUserId(user);
            var manager = await db.Users
                .Where(u => u.Id == managerId)
                .Select(u => new { u.ManagedWarehouseId })
                .FirstOrDefaultAsync();

            if (manager?.ManagedWarehouseId is null)
                return Results.BadRequest(new { Error = "Bạn chưa được gán vào kho nào.", ErrorCode = "NO_WAREHOUSE" });

            var warehouseId = manager.ManagedWarehouseId.Value;

            var pendingCount = await db.Orders.CountAsync(o =>
                o.Status == OrderStatus.Paid_WaitingForBatch);

            var packedCount = await db.Orders.CountAsync(o =>
                o.Status == OrderStatus.PackedAtWarehouse);

            var shippingCount = await db.Orders.CountAsync(o =>
                o.Status == OrderStatus.ShippingToHub);

            var todayStart = DateTime.UtcNow.Date;
            var packedToday = await db.Orders.CountAsync(o =>
                o.PackedAtWarehouseAt >= todayStart);

            var driverCount = await db.Users.CountAsync(u =>
                u.WarehouseId == warehouseId && u.Role == UserRole.Driver && u.IsActive);

            var lowStockCount = await db.Products.CountAsync(p => p.IsActive && p.Stock < 10);

            var warehouse = await db.Warehouses
                .Where(w => w.Id == warehouseId)
                .Select(w => new { w.Id, w.Name, w.Address, w.District, w.Province, w.IsActive })
                .FirstOrDefaultAsync();

            return Results.Ok(new
            {
                Warehouse = warehouse,
                PendingOrders = pendingCount,
                PackedOrders = packedCount,
                ShippingOrders = shippingCount,
                PackedToday = packedToday,
                ActiveDrivers = driverCount,
                LowStockProducts = lowStockCount,
            });
        });

        // GET /warehouse-manager/orders?status=&page=&pageSize= — đơn hàng cần xử lý tại kho
        group.MapGet("/orders", async (
            AppDbContext db,
            string? status = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var targetStatuses = status switch
            {
                "packed" => new[] { OrderStatus.PackedAtWarehouse },
                "shipping" => new[] { OrderStatus.ShippingToHub },
                _ => new[] { OrderStatus.Paid_WaitingForBatch }
            };

            var query = db.Orders
                .Where(o => targetStatuses.Contains(o.Status))
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new
                {
                    o.Id,
                    o.Status,
                    o.TotalAmount,
                    o.CreatedAt,
                    o.PaidAt,
                    o.PackedAtWarehouseAt,
                    o.Note,
                    Hub = new { o.Hub.Id, o.Hub.Name, o.Hub.Address },
                    Customer = new { o.User.FullName, o.User.PhoneNumber },
                    Items = o.Items.Select(i => new
                    {
                        i.ProductId,
                        i.Product.Name,
                        i.Product.ThumbnailUrl,
                        i.Quantity,
                        i.UnitPrice,
                    }),
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            });
        });

        // PATCH /warehouse-manager/orders/{id}/pack — xác nhận đã đóng gói
        group.MapPatch("/orders/{id:guid}/pack", async (Guid id, AppDbContext db) =>
        {
            var order = await db.Orders.FindAsync(id);
            if (order is null)
                return Results.NotFound(new { Error = "Không tìm thấy đơn hàng.", ErrorCode = "ORDER_NOT_FOUND" });

            try
            {
                order.PackAtWarehouse();
                await db.SaveChangesAsync();
                return Results.Ok(new { order.Id, order.Status, order.PackedAtWarehouseAt });
            }
            catch (OrderDomainException ex)
            {
                return Results.Conflict(new { Error = ex.Message, ErrorCode = "INVALID_STATUS" });
            }
        });

        // PATCH /warehouse-manager/orders/pack-batch — đóng gói nhiều đơn cùng lúc
        group.MapPatch("/orders/pack-batch", async ([FromBody] PackBatchRequest body, AppDbContext db) =>
        {
            if (body.OrderIds is not { Count: > 0 })
                return Results.BadRequest(new { Error = "Danh sách đơn hàng không được rỗng." });

            var orders = await db.Orders
                .Where(o => body.OrderIds.Contains(o.Id))
                .ToListAsync();

            var packed = 0;
            var errors = new List<string>();

            foreach (var id in body.OrderIds)
            {
                var order = orders.FirstOrDefault(o => o.Id == id);
                if (order is null) { errors.Add($"Đơn {id}: không tìm thấy."); continue; }
                try { order.PackAtWarehouse(); packed++; }
                catch (OrderDomainException ex) { errors.Add($"Đơn {id}: {ex.Message}"); }
            }

            if (packed > 0) await db.SaveChangesAsync();

            return Results.Ok(new { Packed = packed, Errors = errors });
        });

        // GET /warehouse-manager/inventory?search=&page=&pageSize= — danh sách tồn kho
        group.MapGet("/inventory", async (
            AppDbContext db,
            string? search = null,
            string? filter = null,
            int page = 1,
            int pageSize = 30) =>
        {
            var query = db.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search));

            if (filter == "low")
                query = query.Where(p => p.Stock < 10);
            else if (filter == "out")
                query = query.Where(p => p.Stock == 0);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.Stock)
                .ThenBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.ThumbnailUrl,
                    p.Stock,
                    p.Price,
                    CategoryName = p.Category.Name,
                    IsLowStock = p.Stock < 10,
                    IsOutOfStock = p.Stock == 0,
                })
                .ToListAsync();

            return Results.Ok(new
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            });
        });

        // PATCH /warehouse-manager/inventory/{productId}/adjust — điều chỉnh tồn kho
        group.MapPatch("/inventory/{productId:guid}/adjust", async (
            Guid productId,
            [FromBody] AdjustStockRequest body,
            AppDbContext db) =>
        {
            var product = await db.Products.FindAsync(productId);
            if (product is null)
                return Results.NotFound(new { Error = "Không tìm thấy sản phẩm." });

            var newStock = product.Stock + body.Delta;
            if (newStock < 0)
                return Results.BadRequest(new { Error = $"Tồn kho không thể âm (hiện tại: {product.Stock}, thay đổi: {body.Delta})." });

            product.Stock = newStock;
            await db.SaveChangesAsync();

            return Results.Ok(new { product.Id, product.Name, product.Stock, Delta = body.Delta, Reason = body.Reason });
        });

        // GET /warehouse-manager/drivers — tài xế được gán về kho này
        group.MapGet("/drivers", async (ClaimsPrincipal user, AppDbContext db) =>
        {
            var managerId = GetUserId(user);
            var warehouseId = await db.Users
                .Where(u => u.Id == managerId)
                .Select(u => u.ManagedWarehouseId)
                .FirstOrDefaultAsync();

            if (warehouseId is null)
                return Results.BadRequest(new { Error = "Bạn chưa được gán vào kho nào.", ErrorCode = "NO_WAREHOUSE" });

            var drivers = await db.Users
                .Where(u => u.WarehouseId == warehouseId && u.Role == UserRole.Driver)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.IsActive,
                    ActiveOrders = db.Orders.Count(o =>
                        o.Status == OrderStatus.ShippingToHub),
                })
                .ToListAsync();

            return Results.Ok(drivers);
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record PackBatchRequest(List<Guid> OrderIds);
public record AdjustStockRequest(int Delta, string? Reason);
