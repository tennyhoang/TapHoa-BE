using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Orders.V1.GetAllOrders;

public record GetAllOrdersQuery(
    int Page = 1,
    int PageSize = 10,
    OrderStatus? Status = null,
    string? Search = null)
    : IRequest<Result<PagedResult<AdminOrderResponse>>>;
