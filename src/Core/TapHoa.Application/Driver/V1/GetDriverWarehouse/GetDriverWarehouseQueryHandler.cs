using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1.GetDriverWarehouse;

public class GetDriverWarehouseQueryHandler(
    ICurrentUserService    currentUser,
    IRepository<User>      userRepo)
    : IRequestHandler<GetDriverWarehouseQuery, Result<AssignedWarehouseDto>>
{
    public async Task<Result<AssignedWarehouseDto>> Handle(
        GetDriverWarehouseQuery request, CancellationToken cancellationToken)
    {
        // ── 1. Lấy User kèm AssignedWarehouse trong một lần query ────────
        var driver = await userRepo.Query()
            .Include(u => u.AssignedWarehouse)
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId, cancellationToken);

        if (driver is null)
            return Result<AssignedWarehouseDto>.Fail(
                "Không tìm thấy tài khoản tài xế.", "DRIVER_NOT_FOUND");

        // ── 2. Kiểm tra đã được gán kho chưa ────────────────────────────
        if (driver.AssignedWarehouse is null)
            return Result<AssignedWarehouseDto>.Fail(
                "Tài xế chưa được gán kho cố định. Liên hệ Admin để được phân bổ.",
                "DRIVER_NO_WAREHOUSE");

        var warehouse = driver.AssignedWarehouse;

        // ── 3. Map sang DTO và trả về ────────────────────────────────────
        return Result<AssignedWarehouseDto>.Ok(new AssignedWarehouseDto(
            Id:          warehouse.Id,
            Name:        warehouse.Name,
            FullAddress: $"{warehouse.Address}, {warehouse.Ward}, {warehouse.District}, {warehouse.Province}",
            PhoneNumber: warehouse.PhoneNumber));
    }
}
