using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Orders.V1.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus Status,
    string? CancelReason = null)
    : IRequest<Result<OrderResponse>>;
