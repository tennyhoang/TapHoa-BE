using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Hubs.V1.UpdateHubStatus;

public record UpdateHubStatusCommand(
    Guid HubId,
    HubStatus Status
) : IRequest<Result<HubResponse>>;
