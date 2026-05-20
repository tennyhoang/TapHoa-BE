using MediatR;

namespace TapHoa.Application.Hubs.V1.GetActiveHubs;

public record GetActiveHubsQuery(
    string? City = null,
    string? District = null
) : IRequest<List<HubResponse>>;
