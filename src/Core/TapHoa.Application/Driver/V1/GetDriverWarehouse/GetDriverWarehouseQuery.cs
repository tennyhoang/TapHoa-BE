using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1.GetDriverWarehouse;

/// <summary>
/// Lấy thông tin kho cố định của Driver đang đăng nhập.
/// DriverId được đọc tự động từ ICurrentUserService — không cần truyền vào.
/// </summary>
public record GetDriverWarehouseQuery : IRequest<Result<AssignedWarehouseDto>>;
