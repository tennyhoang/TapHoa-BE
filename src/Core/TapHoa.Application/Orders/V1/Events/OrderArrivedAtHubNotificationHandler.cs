using MediatR;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Contracts;
using TapHoa.Application.Orders.V1.Events;

namespace TapHoa.Application.Orders.V1.Events;

public class OrderArrivedAtHubNotificationHandler(
    IExpoPushService pushService,
    ILogger<OrderArrivedAtHubNotificationHandler> logger)
    : INotificationHandler<OrderArrivedAtHubEvent>
{
    public async Task Handle(OrderArrivedAtHubEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[NOTIFY] Order {OrderId} arrived at Hub '{HubName}'. Notifying user {UserId}.",
            notification.OrderId, notification.HubName, notification.UserId);

        await pushService.SendAsync(
            userId: notification.UserId,
            title:  "Hàng đã đến điểm nhận",
            body:   $"Đơn hàng của bạn đã có tại {notification.HubName}. Vui lòng đến lấy hàng.",
            data:   new { orderId = notification.OrderId, type = "ORDER_IN_HUB" },
            cancellationToken: cancellationToken);
    }
}
