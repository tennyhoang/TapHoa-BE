using Microsoft.AspNetCore.SignalR;
using TapHoa.Application.Contracts;

namespace TapHoa.Api.Hubs;

public class OrderTrackingService(IHubContext<OrderTrackingHub> hubContext) : IOrderTrackingService
{
    public Task NotifyOrderStatusChangedAsync(Guid userId, Guid orderId, string status, CancellationToken cancellationToken = default)
        => hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("OrderStatusChanged", new { orderId, status }, cancellationToken);
}
