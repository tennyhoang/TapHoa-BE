using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Hubs.V1.CreateHub;

public record CreateHubCommand(
    string Name,
    string Address,
    string Ward,
    string District,
    string City,
    double Latitude,
    double Longitude
) : IRequest<Result<HubResponse>>;
