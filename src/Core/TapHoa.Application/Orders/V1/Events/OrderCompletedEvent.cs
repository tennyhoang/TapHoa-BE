using MediatR;

namespace TapHoa.Application.Orders.V1.Events;

public record OrderCompletedEvent(
    Guid OrderId,
    Guid UserId,
    decimal TotalAmount,
    DateTime CompletedAt
) : INotification;
