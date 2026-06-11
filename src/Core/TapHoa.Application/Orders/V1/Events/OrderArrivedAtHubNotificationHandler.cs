using MediatR;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Contracts;
using TapHoa.Application.Orders.V1.Events;

namespace TapHoa.Application.Orders.V1.Events;

public class OrderArrivedAtHubNotificationHandler(
    IExpoPushService pushService,
    IEmailService emailService,
    ILogger<OrderArrivedAtHubNotificationHandler> logger)
    : INotificationHandler<OrderArrivedAtHubEvent>
{
    public async Task Handle(OrderArrivedAtHubEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "[NOTIFY] Order {OrderId} arrived at Hub '{HubName}'. Notifying user {UserId}.",
            notification.OrderId, notification.HubName, notification.UserId);

        var shortRef = notification.OrderId.ToString()[..8].ToUpper();

        await pushService.SendAsync(
            userId: notification.UserId,
            title:  "Hàng đã đến điểm nhận",
            body:   $"Đơn hàng của bạn đã có tại {notification.HubName}. Vui lòng đến lấy hàng.",
            data:   new { orderId = notification.OrderId, type = "ORDER_IN_HUB" },
            cancellationToken: cancellationToken);

        var emailBody = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:0 auto">
              <h2 style="color:#0d9488">Hàng đã đến điểm nhận!</h2>
              <p>Chào {notification.CustomerFullName},</p>
              <p>Đơn hàng <strong>#{shortRef}</strong> đã có tại Hub:</p>
              <div style="padding:12px 16px;background:#f0fdf4;border-radius:8px;margin:12px 0">
                <strong>{notification.HubName}</strong><br/>
                <span style="color:#6b7280">{notification.HubAddress}</span>
              </div>
              <p>Vui lòng đến lấy hàng trong thời gian sớm nhất.</p>
              <p style="color:#6b7280;font-size:13px">Cảm ơn bạn đã tin tưởng TapHoa!</p>
            </div>
            """;
        await emailService.SendEmailAsync(
            notification.CustomerEmail,
            $"Hàng đã đến Hub — Đơn #{shortRef}",
            emailBody,
            cancellationToken);
    }
}
