using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Hubs.V1.GetAllHubs;

public record GetAllHubsQuery(
    int Page = 1,
    int PageSize = 20,
    HubStatus? Status = null,
    string? City = null,
    string? District = null,
    string? Search = null
) : IRequest<PagedResult<HubResponse>>;
