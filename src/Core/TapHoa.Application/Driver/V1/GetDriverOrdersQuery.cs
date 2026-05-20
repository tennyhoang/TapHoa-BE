using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;

namespace TapHoa.Application.Driver.V1;

public record GetDriverOrdersQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<OrderResponse>>>;
