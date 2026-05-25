using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Users.V1.Admin;

public record DeleteUserCommand(Guid UserId, Guid RequesterId) : IRequest<Result<bool>>;
