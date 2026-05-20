using MediatR;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Orders.V1.Events;

namespace TapHoa.Application.Orders.V1.Events;

public class OrderArrivedAtHubNotificationHandler(
    ILogger<OrderArrivedAtHubNotificationHandler> logger)
    : INotificationHandler<OrderArrivedAtHubEvent>
{
    public Task Handle(OrderArrivedAtHubEvent notification, CancellationToken cancellationToken)
    {
        // ── Giả lập gửi thông báo cho khách hàng ──────────────────────────────
        // TODO: Thay thế bằng EmailService / SMS / Push Notification thực tế.
        logger.LogInformation(
            "[NOTIFY] Đơn hàng {OrderId} đã đến Hub '{HubName}' ({HubAddress}) lúc {ArrivedAt:HH:mm dd/MM/yyyy}. " +
            "Gửi thông báo tới khách hàng {CustomerFullName} <{CustomerEmail}>: " +
            "\"Đơn hàng của bạn đã có mặt tại điểm nhận {HubName}. Vui lòng đến lấy hàng.\"",
            notification.OrderId,
            notification.HubName,
            notification.HubAddress,
            notification.ArrivedAt,
            notification.CustomerFullName,
            notification.CustomerEmail,
            notification.HubName);

        return Task.CompletedTask;
    }
}
