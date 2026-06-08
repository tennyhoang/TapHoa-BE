using MediatR;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Contracts;
using TapHoa.Application.Orders.V1.Events;

namespace TapHoa.Application.Orders.V1.Events;

public class OrderCompletedNotificationHandler(
    IExpoPushService pushService,
    ILogger<OrderCompletedNotificationHandler> logger)
    : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ORDER_COMPLETED] Order {OrderId} completed. Notifying user {UserId}.",
            notification.OrderId, notification.UserId);

        await pushService.SendAsync(
            userId: notification.UserId,
            title: "Đơn hàng hoàn thành",
            body:  "Cảm ơn bạn đã mua hàng! Đơn hàng của bạn đã được hoàn tất.",
            data:  new { orderId = notification.OrderId, type = "ORDER_COMPLETED" },
            cancellationToken: cancellationToken);
    }
}
