using TapHoa.Domain.Enums;

namespace TapHoa.Application.Orders.V1;

public record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    string? ThumbnailUrl,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

public record HubInfo(
    Guid Id,
    string Name,
    string Address,
    string Ward,
    string District,
    string City,
    double Latitude,
    double Longitude
);

public record OrderResponse(
    Guid Id,
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
    DateTime? RefundedAt
);
