using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Users.V1.Admin;

public record UpdateUserCommand(
    Guid UserId,
    string? FullName,
    string? PhoneNumber,
    string? Role,
    bool? IsActive,
    Guid? AgentHubId
) : IRequest<Result<AdminUserResponse>>;
