using MediatR;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Contracts;
using TapHoa.Application.Orders.V1.Events;

namespace TapHoa.Application.Orders.V1.Events;

public class OrderCompletedNotificationHandler(
    IExpoPushService pushService,
    IEmailService emailService,
    ILogger<OrderCompletedNotificationHandler> logger)
    : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("[ORDER_COMPLETED] Order {OrderId} completed. Notifying user {UserId}.",
            notification.OrderId, notification.UserId);

        var shortRef = notification.OrderId.ToString()[..8].ToUpper();

        await pushService.SendAsync(
            userId: notification.UserId,
            title: "Đơn hàng hoàn thành",
            body:  "Cảm ơn bạn đã mua hàng! Đơn hàng của bạn đã được hoàn tất.",
            data:  new { orderId = notification.OrderId, type = "ORDER_COMPLETED" },
            cancellationToken: cancellationToken);

        var emailBody = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:0 auto">
              <h2 style="color:#0d9488">Đơn hàng hoàn thành</h2>
              <p>Chào {notification.CustomerFullName},</p>
              <p>Đơn hàng <strong>#{shortRef}</strong> đã được hoàn tất thành công.</p>
              <table style="width:100%;border-collapse:collapse;margin:12px 0">
                <tr><td style="padding:6px 0;color:#6b7280">Tổng giá trị</td><td style="text-align:right;font-weight:bold">{notification.TotalAmount:N0}đ</td></tr>
                <tr><td style="padding:6px 0;color:#6b7280">Ngày hoàn thành</td><td style="text-align:right">{notification.CompletedAt:dd/MM/yyyy HH:mm}</td></tr>
              </table>
              <p style="color:#6b7280;font-size:13px">Cảm ơn bạn đã tin tưởng TapHoa. Hẹn gặp lại!</p>
            </div>
            """;
        await emailService.SendEmailAsync(
            notification.CustomerEmail,
            $"Đơn hàng #{shortRef} đã hoàn thành — TapHoa",
            emailBody,
            cancellationToken);
    }
}
