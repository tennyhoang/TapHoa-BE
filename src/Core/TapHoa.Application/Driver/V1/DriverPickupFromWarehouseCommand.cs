using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1;

public record DriverPickupResult(int Shipped, List<string> Errors);

public record DriverPickupFromWarehouseCommand(
    Guid DriverId,
    List<Guid> OrderIds
) : IRequest<Result<DriverPickupResult>>;
