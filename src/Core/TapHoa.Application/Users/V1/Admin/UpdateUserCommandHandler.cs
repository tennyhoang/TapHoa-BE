using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
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
            Enum.TryParse<Domain.Enums.UserRole>(request.Role, out var role))
        {
            user.Role = role;
            if (role != Domain.Enums.UserRole.Agent)
                user.AgentHubId = null;
        }

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        if (request.AgentHubId.HasValue && user.Role == Domain.Enums.UserRole.Agent)
            user.AgentHubId = request.AgentHubId.Value;

        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        return Result<AdminUserResponse>.Ok(new AdminUserResponse(
            user.Id, user.FullName, user.Email, user.PhoneNumber,
            user.Role.ToString(), user.IsActive, user.AgentHubId, user.CreatedAt));
    }
}
