using MediatR;

namespace TapHoa.Application.UserHubs.V1.GetFavoriteHubs;

public record GetFavoriteHubsQuery(Guid UserId) : IRequest<List<FavoriteHubResponse>>;
