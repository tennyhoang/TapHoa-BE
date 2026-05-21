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
    DateTime? ShippingToHubAt,
    DateTime? InHubAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    DateTime? RefundedAt,
    bool CanStartShipping,
    bool CanMarkInHub,
    bool CanComplete,
    bool CanCancel
);
