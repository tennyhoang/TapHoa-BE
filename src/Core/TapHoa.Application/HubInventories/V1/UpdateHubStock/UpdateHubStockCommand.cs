using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.HubInventories.V1.UpdateHubStock;

public record UpdateHubStockCommand(
    Guid HubId,
    Guid ProductId,
    int NewStock
) : IRequest<Result<HubStockResponse>>;
