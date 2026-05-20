using TapHoa.Domain.Enums;

namespace TapHoa.Application.UserHubs.V1;

public record FavoriteHubResponse(
    Guid Id,
    string Name,
    string Address,
    string Ward,
    string District,
    string City,
    double Latitude,
    double Longitude,
    HubStatus Status,
    bool IsDefault
);
