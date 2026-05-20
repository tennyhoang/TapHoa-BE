using MediatR;

namespace TapHoa.Application.Users.V1.ChangePassword;

public record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword
) : IRequest;
