using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Payment.V1.CreateMomoPayment;

public class CreateMomoPaymentCommandHandler(
    IRepository<Order> orderRepo,
    IMomoService momoService)
    : IRequestHandler<CreateMomoPaymentCommand, MomoPaymentResponse>
{
    public async Task<MomoPaymentResponse> Handle(CreateMomoPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Đơn hàng không tồn tại.");

        if (order.Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException($"Đơn hàng không ở trạng thái chờ thanh toán (trạng thái hiện tại: '{order.Status}').");

        var orderRef = order.PaymentRef ?? "TH" + order.Id.ToString("N")[..8].ToUpper();
        order.PaymentRef = orderRef;
        orderRepo.Update(order);
        await orderRepo.SaveChangesAsync();

        var response = await momoService.CreatePaymentAsync(order.TotalAmount, orderRef);

        return response;
    }
}
