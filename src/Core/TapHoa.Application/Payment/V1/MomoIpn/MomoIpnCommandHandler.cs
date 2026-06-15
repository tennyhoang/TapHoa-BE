using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Payment.V1.MomoIpn;

public class MomoIpnCommandHandler(
    IRepository<Order> orderRepo,
    IMomoService momoService)
    : IRequestHandler<MomoIpnCommand, bool>
{
    public async Task<bool> Handle(MomoIpnCommand request, CancellationToken cancellationToken)
    {
        var parameters = request.Parameters;

        if (!parameters.TryGetValue("signature", out var signature)
            || !parameters.TryGetValue("orderId", out var orderId)
            || !parameters.TryGetValue("errorCode", out var errorCode))
            return false;

        if (!momoService.VerifyIpn(parameters, signature))
            return false;

        var order = await orderRepo.Query()
            .FirstOrDefaultAsync(o => o.PaymentRef == orderId.Split('_')[0] && o.Status == OrderStatus.AwaitingPayment, cancellationToken);

        if (order is null)
            return false;

        if (errorCode == "0")
        {
            order.ConfirmPayment();
            orderRepo.Update(order);
            await orderRepo.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
