using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Claims.V1.CreateClaim;

public record CreateClaimCommand(
    Guid OrderId,
    Guid CustomerId,
    string Reason,
    string? ImageUrl
) : IRequest<Result<ClaimResponse>>;
