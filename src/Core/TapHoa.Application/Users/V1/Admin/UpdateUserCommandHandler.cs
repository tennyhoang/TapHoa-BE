using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.Admin;

public class UpdateUserCommandHandler(IRepository<User> userRepo)
    : IRequestHandler<UpdateUserCommand, Result<AdminUserResponse>>
{
    public async Task<Result<AdminUserResponse>> Handle(
        UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.FindAsync(u => u.Id == request.UserId);
        if (user is null)
            return Result<AdminUserResponse>.Fail("Không tìm thấy người dùng.", "USER_NOT_FOUND");

        if (!string.IsNullOrWhiteSpace(request.FullName))
            user.FullName = request.FullName;

        if (request.PhoneNumber is not null)
            user.PhoneNumber = request.PhoneNumber;

        if (!string.IsNullOrWhiteSpace(request.Role) &&
            Enum.TryParse<UserRole>(request.Role, out var newRole))
        {
            var previousRole = user.Role;
            user.Role = newRole;

            // Khi rời khỏi Agent, xoá Hub được gán
            if (newRole != UserRole.Agent)
                user.AgentHubId = null;

            // Khi rời khỏi Driver, xoá Kho xuất phát
            if (previousRole == UserRole.Driver && newRole != UserRole.Driver)
                user.WarehouseId = null;

            // Khi rời khỏi WarehouseManager, xoá Kho quản lý
            if (previousRole == UserRole.WarehouseManager && newRole != UserRole.WarehouseManager)
                user.ManagedWarehouseId = null;
        }

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        if (request.AgentHubId.HasValue && user.Role == UserRole.Agent)
            user.AgentHubId = request.AgentHubId.Value;

        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        // WarehouseName/ManagedWarehouseName trả null — navigation chưa load, list page sẽ refetch
        return Result<AdminUserResponse>.Ok(new AdminUserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Role.ToString(),
            user.IsActive,
            user.AgentHubId,
            user.CreatedAt,
            user.WarehouseId,
            null,
            user.ManagedWarehouseId,
            null));
    }
}
