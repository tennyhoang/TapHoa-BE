using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;
using TapHoa.Persistence.Data;

namespace TapHoa.Persistence.Repositories;

public class LoyaltyRepository(AppDbContext db) : ILoyaltyRepository
{
    public Task<LoyaltyAccount?> GetAccountAsync(Guid userId, CancellationToken ct = default)
        => db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId, ct);

    public async Task EarnAsync(Guid userId, int points, Guid? orderId, string description, CancellationToken ct = default)
    {
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId, ct);
        if (account is null)
        {
            account = new LoyaltyAccount { UserId = userId };
            db.LoyaltyAccounts.Add(account);
        }

        account.Earn(points);
        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId      = userId,
            Type        = "Earned",
            Points      = points,
            OrderId     = orderId,
            Description = description,
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task RedeemAsync(Guid userId, int points, Guid orderId, CancellationToken ct = default)
    {
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(la => la.UserId == userId, ct)
            ?? throw new InvalidOperationException("Tài khoản điểm tích lũy không tồn tại.");

        account.Redeem(points);
        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId      = userId,
            Type        = "Redeemed",
            Points      = points,
            OrderId     = orderId,
            Description = $"Đổi điểm đơn hàng #{orderId.ToString()[..8].ToUpper()}",
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task<List<LoyaltyTransaction>> GetTransactionsAsync(
        Guid userId, int page, int pageSize, CancellationToken ct = default)
        => await db.LoyaltyTransactions
            .Where(lt => lt.UserId == userId)
            .OrderByDescending(lt => lt.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public Task<int> CountTransactionsAsync(Guid userId, CancellationToken ct = default)
        => db.LoyaltyTransactions.CountAsync(lt => lt.UserId == userId, ct);
}
