using Microsoft.EntityFrameworkCore;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Warehouses;

public static class WarehouseEndpoints
{
    public static void MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/warehouses").WithTags("Warehouses");

        // GET /api/v1/warehouses — danh sách kho đang hoạt động (driver chọn kho xuất phát)
        group.MapGet("/", async (AppDbContext db) =>
        {
            var warehouses = await db.Warehouses
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new
                {
                    w.Id, w.Name, w.Address, w.Ward, w.District, w.Province, w.PhoneNumber,
                })
                .ToListAsync();
            return Results.Ok(warehouses);
        });
    }
}
