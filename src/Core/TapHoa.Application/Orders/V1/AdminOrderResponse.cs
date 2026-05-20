using TapHoa.Domain.Enums;

namespace TapHoa.Application.Orders.V1;

public record AdminOrderResponse(
    Guid Id,
    Guid UserId,
    string UserFullName,
    string CustomerEmail,
    OrderStatus Status,
    decimal TotalAmount,
    string? Note,
    HubInfo Hub,
    List<OrderItemResponse> Items,
    DateTime CreatedAt,
    string? CancelReason,
    DateTime? ConfirmedAt,
    DateTime? ShippingAt,
    DateTime? ArrivedAtHubAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    DateTime? RefundedAt,
    bool CanConfirm,
    bool CanShip,
    bool CanArriveAtHub,
    bool CanDeliver,
    bool CanCancel
);
