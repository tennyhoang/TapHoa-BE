using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;
using TapHoa.Persistence.Data;

namespace TapHoa.Persistence.Repositories;

public class FlashSaleRepository(AppDbContext db) : Repository<FlashSaleItem>(db), IFlashSaleRepository
{
    public async Task<List<FlashSaleItem>> GetActiveByProductIdsAsync(
        IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await db.FlashSaleItems
            .Include(f => f.Session)
            .Where(f => productIds.Contains(f.ProductId)
                && f.Session.IsActive
                && f.Session.StartTime <= now
                && f.Session.EndTime >= now)
            .ToListAsync(ct);
    }

    public async Task<bool> TryDecrementStockAsync(Guid itemId, int quantity, CancellationToken ct = default)
    {
        var rows = await db.Database.ExecuteSqlAsync(
            $"""
            UPDATE "FlashSaleItems"
            SET "FlashSaleStock" = "FlashSaleStock" - {quantity},
                "SoldCount" = "SoldCount" + {quantity}
            WHERE "Id" = {itemId}
              AND "FlashSaleStock" >= {quantity}
            """,
            ct);
        return rows == 1;
    }
}
