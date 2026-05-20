using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.UserHubs.V1.AddFavoriteHub;

public record AddFavoriteHubCommand(Guid UserId, Guid HubId) : IRequest<Result<FavoriteHubResponse>>;
