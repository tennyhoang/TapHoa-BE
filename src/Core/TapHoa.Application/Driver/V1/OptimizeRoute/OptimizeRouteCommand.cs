using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1.OptimizeRoute;

public record OptimizeRouteCommand(
    string       HubAddress,
    List<string> OrderAddresses
) : IRequest<Result<OptimizeRouteResponse>>;
