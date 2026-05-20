using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;

namespace TapHoa.Application.Agent.V1;

public record AgentArriveCommand(
    Guid OrderId,
    Guid AgentUserId,
    Guid AgentHubId
) : IRequest<Result<OrderResponse>>;
