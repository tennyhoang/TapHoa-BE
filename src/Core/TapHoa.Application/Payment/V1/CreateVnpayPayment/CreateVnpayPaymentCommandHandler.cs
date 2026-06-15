using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Payment.V1.CreateVnpayPayment;

public class CreateVnpayPaymentCommandHandler(
    IRepository<Order> orderRepo,
    IVnpayService vnpayService)
    : IRequestHandler<CreateVnpayPaymentCommand, VnpayPaymentResponse>
{
    public async Task<VnpayPaymentResponse> Handle(CreateVnpayPaymentCommand request, CancellationToken cancellationToken)
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

        var ipAddress = GetLocalIpAddress();
        var paymentUrl = vnpayService.CreatePaymentUrl(order.TotalAmount, orderRef, ipAddress);

        return new VnpayPaymentResponse(paymentUrl, orderRef);
    }

    private static string GetLocalIpAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        var ip = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
        return ip?.ToString() ?? "127.0.0.1";
    }
}
