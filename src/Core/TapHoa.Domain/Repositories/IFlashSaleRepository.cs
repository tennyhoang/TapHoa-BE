using TapHoa.Domain.Entities;

namespace TapHoa.Domain.Repositories;

public interface IFlashSaleRepository : IRepository<FlashSaleItem>
{
    Task<List<FlashSaleItem>> GetActiveByProductIdsAsync(IEnumerable<Guid> productIds, CancellationToken ct = default);

    /// <summary>
    /// Atomically decrements FlashSaleStock and increments SoldCount using UPDATE ... WHERE FlashSaleStock >= quantity.
    /// Returns false if stock is insufficient (concurrent race or genuine out-of-stock).
    /// </summary>
    Task<bool> TryDecrementStockAsync(Guid itemId, int quantity, CancellationToken ct = default);
}
