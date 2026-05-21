using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;

namespace TapHoa.Application.Driver.V1;

public record DriverHubBatch(
    Guid HubId,
    string HubName,
    string HubFullAddress,
    int OrderCount,
    decimal TotalAmount,
    List<OrderResponse> Orders
);

public record GetDriverOrdersQuery() : IRequest<Result<List<DriverHubBatch>>>;
