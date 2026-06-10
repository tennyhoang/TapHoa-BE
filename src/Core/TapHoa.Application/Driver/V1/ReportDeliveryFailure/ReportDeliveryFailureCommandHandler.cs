using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1.ReportDeliveryFailure;

public class ReportDeliveryFailureCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletRepo,
    IRepository<UserNotification> notificationRepo,
    IOrderStatusBroadcaster broadcaster)
    : IRequestHandler<ReportDeliveryFailureCommand, Result<DeliveryFailureResponse>>
{
    public async Task<Result<DeliveryFailureResponse>> Handle(
        ReportDeliveryFailureCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.User)
            .Include(o => o.Hub)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<DeliveryFailureResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        bool autoCancelled;
        try
        {
            autoCancelled = order.RecordDeliveryFailure(request.Reason);
        }
        catch (OrderDomainException ex)
        {
            return Result<DeliveryFailureResponse>.Fail(ex.Message, "INVALID_TRANSITION");
        }

        await orderRepo.SaveChangesAsync();

        if (autoCancelled)
        {
            // Hoàn tiền ngay vào ví (BR-007: T+1 simplified to immediate)
            var user = order.User;
            user.CreditWallet(order.TotalAmount);
            await userRepo.SaveChangesAsync();

            var shortRef = order.Id.ToString()[..8].ToUpper();

            await walletRepo.AddAsync(new WalletTransaction
            {
                UserId      = order.UserId,
                Amount      = order.TotalAmount,
                Type        = WalletTransactionType.Credit,
                Description = $"Hoàn tiền đơn #{shortRef} — giao thất bại 3 lần",
                OrderId     = order.Id,
            });
            await walletRepo.SaveChangesAsync();

            await notificationRepo.AddAsync(new UserNotification
            {
                UserId = order.UserId,
                Type   = "OrderStatus",
                Title  = $"Đơn hàng #{shortRef} đã bị huỷ tự động",
                Body   = "Đơn hàng không thể giao sau 3 lần thử. Tiền đã được hoàn vào ví của bạn.",
                Data   = System.Text.Json.JsonSerializer.Serialize(new { orderId = order.Id.ToString() }),
            });
            await notificationRepo.SaveChangesAsync();

            await broadcaster.BroadcastStatusChanged(
                order.Id, order.Status.ToString(), order.UserId, cancellationToken);
        }

        var remaining = 3 - order.FailedDeliveryAttempts;
        var message = autoCancelled
            ? "Đơn đã bị huỷ tự động và hoàn tiền vào ví khách hàng."
            : $"Ghi nhận lần thất bại thứ {order.FailedDeliveryAttempts}. Còn {remaining} lần trước khi tự động huỷ.";

        return Result<DeliveryFailureResponse>.Ok(
            new DeliveryFailureResponse(order.FailedDeliveryAttempts, autoCancelled, message));
    }
}
