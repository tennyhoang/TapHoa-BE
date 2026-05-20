using MediatR;

namespace TapHoa.Application.Orders.V1.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId, Guid UserId) : IRequest<OrderResponse>;
