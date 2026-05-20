using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Orders.V1.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId,
    Guid UserId,
    string? CancelReason = null)
    : IRequest<Result<OrderResponse>>;
