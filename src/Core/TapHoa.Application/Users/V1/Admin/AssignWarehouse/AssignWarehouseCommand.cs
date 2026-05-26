using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Users.V1.Admin.AssignWarehouse;

/// <summary>
/// Gán (hoặc gỡ) kho cho một User theo TargetType.
/// - TargetType = "Driver"  → gán User.WarehouseId         (kho xuất phát của tài xế)
/// - TargetType = "Manager" → gán User.ManagedWarehouseId  (kho phụ trách của quản lý kho)
/// WarehouseId = null nghĩa là gỡ kho khỏi user.
/// </summary>
public record AssignWarehouseCommand(
    Guid    UserId,
    Guid?   WarehouseId,
    string  TargetType
) : IRequest<Result<AssignWarehouseResponse>>;
