using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1.OptimizeRoute;

/// <summary>
/// Tối ưu lộ trình giao hàng cho tài xế.
/// Backend tự tra kho xuất phát từ WarehouseId được gán trên tài khoản Driver.
/// </summary>
public record OptimizeRouteCommand(
    Guid         DriverId,
    List<string> OrderAddresses
) : IRequest<Result<OptimizeRouteResponse>>;
