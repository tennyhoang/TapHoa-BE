using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Notifications.V1.RegisterPushToken;

public record RegisterPushTokenCommand(Guid UserId, string Token, string? Platform) : IRequest<Result<bool>>;
