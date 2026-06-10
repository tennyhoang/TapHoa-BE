using TapHoa.Domain.Enums;

namespace TapHoa.Application.Hubs.V1;

public record HubResponse(
    Guid Id,
    string Name,
    string Address,
    string Ward,
    string District,
    string City,
    double Latitude,
    double Longitude,
    HubStatus Status,
    decimal MinimumOrderAmount,
    decimal FreeShippingThreshold,
    decimal ShippingFee
);
