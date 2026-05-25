using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;

namespace TapHoa.Application.Driver.V1;

public record GetDriverShippingOrdersQuery(int Page = 1, int PageSize = 50)
    : IRequest<Result<PagedResult<OrderResponse>>>;
