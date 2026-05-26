using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.Admin.AssignWarehouse;

public class AssignWarehouseCommandHandler(
    IRepository<User>      userRepo,
    IRepository<Warehouse> warehouseRepo)
    : IRequestHandler<AssignWarehouseCommand, Result<AssignWarehouseResponse>>
{
    public async Task<Result<AssignWarehouseResponse>> Handle(
        AssignWarehouseCommand request, CancellationToken cancellationToken)
    {
        // ── 1. Tìm User ───────────────────────────────────────────────────
        var user = await userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return Result<AssignWarehouseResponse>.Fail(
                "Không tìm thấy người dùng.", "USER_NOT_FOUND");

        // ── 2. Kiểm tra Role phù hợp với TargetType ──────────────────────
        var isTargetDriver  = string.Equals(request.TargetType, "Driver",  StringComparison.OrdinalIgnoreCase);
        var isTargetManager = string.Equals(request.TargetType, "Manager", StringComparison.OrdinalIgnoreCase);

        if (isTargetDriver && user.Role != UserRole.Driver)
            return Result<AssignWarehouseResponse>.Fail(
                $"Không thể gán kho tài xế cho tài khoản có role '{user.Role}'. " +
                "Chỉ áp dụng cho Driver.",
                "ROLE_MISMATCH");

        if (isTargetManager && user.Role != UserRole.WarehouseManager)
            return Result<AssignWarehouseResponse>.Fail(
                $"Không thể gán kho quản lý cho tài khoản có role '{user.Role}'. " +
                "Chỉ áp dụng cho WarehouseManager.",
                "ROLE_MISMATCH");

        // ── 3. Nếu gán kho (không phải gỡ), kiểm tra kho tồn tại và đang hoạt động ──
        if (request.WarehouseId.HasValue)
        {
            var warehouse = await warehouseRepo.GetByIdAsync(request.WarehouseId.Value);
            if (warehouse is null)
                return Result<AssignWarehouseResponse>.Fail(
                    "Kho không tồn tại.", "WAREHOUSE_NOT_FOUND");

            if (!warehouse.IsActive)
                return Result<AssignWarehouseResponse>.Fail(
                    $"Kho '{warehouse.Name}' đang bị vô hiệu hoá. Chỉ có thể gán kho đang hoạt động.",
                    "WAREHOUSE_INACTIVE");
        }

        // ── 4. Thực hiện gán / gỡ kho ────────────────────────────────────
        if (isTargetDriver)
            user.WarehouseId = request.WarehouseId;
        else
            user.ManagedWarehouseId = request.WarehouseId;

        // ── 5. Lưu vào DB ─────────────────────────────────────────────────
        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        return Result<AssignWarehouseResponse>.Ok(new AssignWarehouseResponse(
            UserId:            user.Id,
            FullName:          user.FullName,
            Role:              user.Role.ToString(),
            WarehouseId:       user.WarehouseId,
            ManagedWarehouseId: user.ManagedWarehouseId));
    }
}
