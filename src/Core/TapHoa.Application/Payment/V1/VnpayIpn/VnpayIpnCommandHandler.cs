using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Payment.V1.VnpayIpn;

public class VnpayIpnCommandHandler(
    IRepository<Order> orderRepo,
    IVnpayService vnpayService)
    : IRequestHandler<VnpayIpnCommand, bool>
{
    public async Task<bool> Handle(VnpayIpnCommand request, CancellationToken cancellationToken)
    {
        var parameters = request.Parameters;

        if (!parameters.TryGetValue("vnp_SecureHash", out var secureHash)
            || !parameters.TryGetValue("vnp_TxnRef", out var txnRef)
            || !parameters.TryGetValue("vnp_ResponseCode", out var responseCode))
            return false;

        if (!vnpayService.VerifyIpn(parameters, secureHash))
            return false;

        var order = await orderRepo.Query()
            .FirstOrDefaultAsync(o => o.PaymentRef == txnRef && o.Status == OrderStatus.AwaitingPayment, cancellationToken);

        if (order is null)
            return false;

        if (responseCode == "00")
        {
            order.ConfirmPayment();
            orderRepo.Update(order);
            await orderRepo.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
