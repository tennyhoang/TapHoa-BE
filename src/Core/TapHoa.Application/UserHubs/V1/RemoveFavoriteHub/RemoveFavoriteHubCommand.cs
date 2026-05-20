using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.UserHubs.V1.RemoveFavoriteHub;

public record RemoveFavoriteHubCommand(Guid UserId, Guid HubId) : IRequest<Result<bool>>;
