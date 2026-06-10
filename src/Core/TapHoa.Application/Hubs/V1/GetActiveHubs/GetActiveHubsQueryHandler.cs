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

        var hubs = await query.ToListAsync(cancellationToken);

        // BR-009: sort nearest-first when caller provides coordinates
        if (request.Latitude.HasValue && request.Longitude.HasValue)
        {
            var lat = request.Latitude.Value;
            var lng = request.Longitude.Value;
            hubs = [.. hubs.OrderBy(h => Haversine(lat, lng, h.Latitude, h.Longitude))];
        }
        else
        {
            hubs = [.. hubs.OrderBy(h => h.City).ThenBy(h => h.District).ThenBy(h => h.Name)];
        }

        return hubs.Select(MapToResponse).ToList();
    }

    // Returns distance in km
    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRad(double deg) => deg * Math.PI / 180;

    internal static HubResponse MapToResponse(Hub h) => new(
        h.Id, h.Name, h.Address, h.Ward, h.District, h.City,
        h.Latitude, h.Longitude, h.Status,
        h.MinimumOrderAmount, h.FreeShippingThreshold, h.ShippingFee);
}
