using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TapHoa.Application.Loyalty;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.Events;

public class EarnLoyaltyPointsHandler(
    ILoyaltyRepository loyaltyRepo,
    ILogger<EarnLoyaltyPointsHandler> logger,
    IOptions<LoyaltyOptions> options)
    : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var pointsEarned = (int)Math.Floor(notification.TotalAmount / options.Value.EarnPerUnit);
        if (pointsEarned <= 0) return;

        await loyaltyRepo.EarnAsync(
            notification.UserId,
            pointsEarned,
            notification.OrderId,
            $"Tích điểm đơn hàng #{notification.OrderId.ToString()[..8].ToUpper()}",
            cancellationToken);

        logger.LogInformation(
            "[LOYALTY] User {UserId} earned {Points} points from order {OrderId}.",
            notification.UserId, pointsEarned, notification.OrderId);
    }
}
