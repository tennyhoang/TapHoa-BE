using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Payment.V1.ProcessRefund;

public class ProcessRefundCommandHandler(
    IRepository<Order> orderRepo,
    IVnpayService vnpayService,
    IMomoService momoService)
    : IRequestHandler<ProcessRefundCommand, Result<ProcessRefundResult>>
{
    public async Task<Result<ProcessRefundResult>> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<ProcessRefundResult>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        if (order.Status != OrderStatus.Completed && order.Status != OrderStatus.Cancelled)
            return Result<ProcessRefundResult>.Fail(
                "Chỉ có thể hoàn tiền cho đơn hàng đã hoàn thành hoặc đã hủy.", "INVALID_STATUS");

        if (string.IsNullOrWhiteSpace(order.PaymentMethod) || order.PaidAt is null)
            return Result<ProcessRefundResult>.Fail(
                "Đơn hàng không có thông tin thanh toán để hoàn tiền.", "NO_PAYMENT_INFO");

        try
        {
            bool refundSuccess;

            switch (order.PaymentMethod)
            {
                case "Vnpay":
                    refundSuccess = await vnpayService.RefundAsync(
                        order.TotalAmount,
                        order.PaymentRef ?? string.Empty,
                        string.Empty,   // transactionNo — cần lưu từ IPN
                        order.PaidAt.Value.ToString("yyyyMMddHHmmss"));
                    break;

                case "Momo":
                    refundSuccess = await momoService.RefundAsync(
                        order.PaymentRef ?? string.Empty,
                        order.TotalAmount,
                        0);
                    break;

                default:
                    return Result<ProcessRefundResult>.Fail(
                        $"Phương thức thanh toán '{order.PaymentMethod}' chưa hỗ trợ hoàn tiền tự động.", "UNSUPPORTED_METHOD");
            }

            if (!refundSuccess)
                return Result<ProcessRefundResult>.Fail("Cổng thanh toán từ chối yêu cầu hoàn tiền.", "REFUND_REJECTED");

            order.Refund();
            orderRepo.Update(order);
            await orderRepo.SaveChangesAsync();

            return Result<ProcessRefundResult>.Ok(new ProcessRefundResult("Hoàn tiền thành công."));
        }
        catch (OrderDomainException ex)
        {
            return Result<ProcessRefundResult>.Fail(ex.Message, "INVALID_TRANSITION");
        }
    }
}
