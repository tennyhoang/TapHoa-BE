using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.ConfirmPayment;

public class ConfirmPaymentCommandHandler(IRepository<Order> orderRepo)
    : IRequestHandler<ConfirmPaymentCommand, bool>
{
    public async Task<bool> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
    {
        // Chỉ xử lý giao dịch tiền vào
        if (request.Payload.TransferType != "in")
            return false;

        // Tìm mã tham chiếu trong nội dung chuyển khoản (e.g. "TH2685894A")
        var content = request.Payload.Content?.ToUpper() ?? string.Empty;
        var order = await orderRepo.Query()
            .Where(o => o.Status == OrderStatus.PendingPayment
                     && o.PaymentRef != null
                     && content.Contains(o.PaymentRef.ToUpper()))
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
            return false;

        order.ConfirmPayment();
        orderRepo.Update(order);
        await orderRepo.SaveChangesAsync();

        return true;
    }
}
