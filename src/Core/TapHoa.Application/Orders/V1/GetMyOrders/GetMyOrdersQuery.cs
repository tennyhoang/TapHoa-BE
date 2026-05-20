using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Orders.V1.GetMyOrders;

public record GetMyOrdersQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10,
    OrderStatus? Status = null,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null,
    string? SortByAmount = null)
    : IRequest<Result<PagedResult<OrderResponse>>>;
