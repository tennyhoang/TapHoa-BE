using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Hubs.V1.DeleteHub;

public record DeleteHubCommand(Guid Id) : IRequest<Result<bool>>;
