using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Hubs.V1.GetActiveHubs;

public class GetActiveHubsQueryHandler(IRepository<Hub> hubRepo)
    : IRequestHandler<GetActiveHubsQuery, List<HubResponse>>
{
    public async Task<List<HubResponse>> Handle(GetActiveHubsQuery request, CancellationToken cancellationToken)
    {
        var query = hubRepo.Query().Where(h => h.Status == HubStatus.Active);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(h => h.City.ToLower().Contains(request.City.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.District))
            query = query.Where(h => h.District.ToLower().Contains(request.District.ToLower()));

        var hubs = await query
            .OrderBy(h => h.City)
            .ThenBy(h => h.District)
            .ThenBy(h => h.Name)
            .ToListAsync(cancellationToken);

        return hubs.Select(MapToResponse).ToList();
    }

    internal static HubResponse MapToResponse(Hub h) => new(
        h.Id, h.Name, h.Address, h.Ward, h.District, h.City,
        h.Latitude, h.Longitude, h.Status,
        h.MinimumOrderAmount, h.FreeShippingThreshold, h.ShippingFee);
}
