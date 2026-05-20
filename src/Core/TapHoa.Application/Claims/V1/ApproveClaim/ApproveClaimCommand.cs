using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Claims.V1.ApproveClaim;

public record ApproveClaimCommand(Guid ClaimId) : IRequest<Result<ClaimResponse>>;
