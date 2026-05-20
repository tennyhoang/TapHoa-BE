using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Hubs.V1.UpdateHub;

public record UpdateHubCommand(
    Guid Id,
    string Name,
    string Address,
    string Ward,
    string District,
    string City,
    double Latitude,
    double Longitude
) : IRequest<Result<HubResponse>>;
