using MediatR;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Orders.V1.Events;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.CashbackOnOrderCompleted;

public class CashbackOnOrderCompletedHandler(
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletTransactionRepo,
    ILogger<CashbackOnOrderCompletedHandler> logger)
    : INotificationHandler<OrderCompletedEvent>
{
    private const decimal CashbackRate = 0.02m;

    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(notification.UserId);
        if (user is null)
        {
            logger.LogWarning(
                "Cashback skipped: user {UserId} not found for order {OrderId}",
                notification.UserId, notification.OrderId);
            return;
        }

        var cashbackAmount = Math.Round(notification.TotalAmount * CashbackRate, 0);
        if (cashbackAmount <= 0)
            return;

        user.CreditWallet(cashbackAmount);

        var shortOrderRef = notification.OrderId.ToString()[..8].ToUpper();
        var transaction = new WalletTransaction
        {
            UserId      = user.Id,
            Amount      = cashbackAmount,
            Type        = WalletTransactionType.Credit,
            Description = $"Cashback 2% đơn hàng #{shortOrderRef}",
            OrderId     = notification.OrderId,
        };

        await walletTransactionRepo.AddAsync(transaction);
        await walletTransactionRepo.SaveChangesAsync();

        logger.LogInformation(
            "Cashback credited: userId={UserId} amount={Amount} orderId={OrderId}",
            user.Id, cashbackAmount, notification.OrderId);
    }
}
