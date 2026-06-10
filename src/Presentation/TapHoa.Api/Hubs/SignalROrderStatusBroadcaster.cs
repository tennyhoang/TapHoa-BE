using Microsoft.AspNetCore.SignalR;
using TapHoa.Application.Contracts;

namespace TapHoa.Api.Hubs;

public class SignalROrderStatusBroadcaster(IHubContext<OrderTrackingHub> hubContext)
    : IOrderStatusBroadcaster
{
    public async Task BroadcastStatusChanged(Guid orderId, string status, Guid userId, CancellationToken ct = default)
    {
        var payload = new OrderStatusChangedPayload(orderId.ToString(), status);
        await hubContext.Clients.Group(userId.ToString())
            .SendAsync("OrderStatusChanged", payload, ct);
    }
}
