using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Agent.V1;

public record GetAgentOrdersQuery(
    Guid HubId,
    OrderStatus? Status = null,
    int Page = 1,
    int PageSize = 20)
    : IRequest<Result<PagedResult<OrderResponse>>>;
