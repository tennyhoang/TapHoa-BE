using TapHoa.Domain.Entities;

namespace TapHoa.Domain.Repositories;

public interface ILoyaltyRepository
{
    Task<LoyaltyAccount?> GetAccountAsync(Guid userId, CancellationToken ct = default);
    Task EarnAsync(Guid userId, int points, Guid? orderId, string description, CancellationToken ct = default);
    Task RedeemAsync(Guid userId, int points, Guid orderId, CancellationToken ct = default);
    Task<List<LoyaltyTransaction>> GetTransactionsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountTransactionsAsync(Guid userId, CancellationToken ct = default);
}
