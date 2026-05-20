using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.UserHubs.V1.GetFavoriteHubs;

public class GetFavoriteHubsQueryHandler(IRepository<UserHub> userHubRepo)
    : IRequestHandler<GetFavoriteHubsQuery, List<FavoriteHubResponse>>
{
    public async Task<List<FavoriteHubResponse>> Handle(GetFavoriteHubsQuery request, CancellationToken cancellationToken)
    {
        var userHubs = await userHubRepo.Query()
            .Include(uh => uh.Hub)
            .Where(uh => uh.UserId == request.UserId)
            .OrderByDescending(uh => uh.IsDefault)
            .ThenBy(uh => uh.CreatedAt)
            .ToListAsync(cancellationToken);

        return userHubs.Select(uh => new FavoriteHubResponse(
            uh.Hub.Id, uh.Hub.Name, uh.Hub.Address, uh.Hub.Ward, uh.Hub.District, uh.Hub.City,
            uh.Hub.Latitude, uh.Hub.Longitude, uh.Hub.Status, uh.IsDefault
        )).ToList();
    }
}
