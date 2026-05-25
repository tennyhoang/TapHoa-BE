using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.ConfirmPayment;

public class ConfirmPaymentCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<WalletTopupRequest> topupRepo,
    IRepository<WalletTransaction> walletTransactionRepo)
    : IRequestHandler<ConfirmPaymentCommand, bool>
{
    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Payload.TransferType != "in")
            return false;

        var content = request.Payload.Content?.ToUpper() ?? string.Empty;

        // ── Order payment ────────────────────────────────────────────────────
        var order = await orderRepo.Query()
            .Where(o => o.Status == OrderStatus.PendingPayment
                     && o.PaymentRef != null
                     && content.Contains(o.PaymentRef.ToUpper()))
            .FirstOrDefaultAsync(cancellationToken);

        if (order is not null)
        {
            order.ConfirmPayment();
            orderRepo.Update(order);
            await orderRepo.SaveChangesAsync();
            return true;
        }

        // ── Wallet top-up (ref starts with WLT) ─────────────────────────────
        if (!content.Contains("THWL"))
            return false;

        var topup = await topupRepo.Query()
            .Include(t => t.User)
            .Where(t => t.Status == WalletTopupStatus.Pending
                     && content.Contains(t.PaymentRef.ToUpper()))
            .FirstOrDefaultAsync(cancellationToken);

        if (topup is null)
            return false;

        var credited = request.Payload.TransferAmount;
        topup.User.CreditWallet(credited);
        topup.Complete();
        await topupRepo.SaveChangesAsync(); // saves User balance + topup status

        await walletTransactionRepo.AddAsync(new WalletTransaction
        {
            UserId      = topup.UserId,
            Amount      = credited,
            Type        = WalletTransactionType.Credit,
            Description = "Nạp tiền vào ví qua chuyển khoản",
        });
        await walletTransactionRepo.SaveChangesAsync();

        return true;
    }
}
