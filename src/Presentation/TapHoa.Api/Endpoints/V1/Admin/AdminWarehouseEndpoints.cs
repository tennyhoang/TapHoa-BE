using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminWarehouseEndpoints
{
    public static void MapAdminWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/warehouses")
            .WithTags("Admin - Warehouses")
            .RequireAuthorization("Admin");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var warehouses = await db.Warehouses
                .OrderBy(w => w.Name)
                .Select(w => new
                {
                    w.Id, w.Name, w.Address, w.Ward, w.District, w.Province,
                    w.PhoneNumber, w.IsActive, w.CreatedAt,
                })
                .ToListAsync();
            return Results.Ok(warehouses);
        });

        group.MapPost("/", async ([FromBody] WarehouseRequest req, AppDbContext db) =>
        {
            var warehouse = new Warehouse
            {
                Name        = req.Name,
                Address     = req.Address,
                Ward        = req.Ward,
                District    = req.District,
                Province    = req.Province,
                PhoneNumber = req.PhoneNumber,
                IsActive    = true,
            };
            db.Warehouses.Add(warehouse);
            await db.SaveChangesAsync();
            return Results.Ok(new { warehouse.Id });
        });

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] WarehouseRequest req, AppDbContext db) =>
        {
            var warehouse = await db.Warehouses.FindAsync(id);
            if (warehouse is null) return Results.NotFound();

            warehouse.Name        = req.Name;
            warehouse.Address     = req.Address;
            warehouse.Ward        = req.Ward;
            warehouse.District    = req.District;
            warehouse.Province    = req.Province;
            warehouse.PhoneNumber = req.PhoneNumber;
            await db.SaveChangesAsync();
            return Results.Ok(new { warehouse.Id });
        });

        group.MapPatch("/{id:guid}/toggle", async (Guid id, AppDbContext db) =>
        {
            var warehouse = await db.Warehouses.FindAsync(id);
            if (warehouse is null) return Results.NotFound();

            warehouse.IsActive = !warehouse.IsActive;
            await db.SaveChangesAsync();
            return Results.Ok(new { warehouse.IsActive });
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var warehouse = await db.Warehouses.FindAsync(id);
            if (warehouse is null) return Results.NotFound();

            db.Warehouses.Remove(warehouse);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }
}

public record WarehouseRequest(
    string  Name,
    string  Address,
    string  Ward,
    string  District,
    string  Province,
    string? PhoneNumber);
