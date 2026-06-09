using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Claims.V1.GetMyClaims;

public record GetMyClaimsQuery(Guid CustomerId) : IRequest<Result<List<ClaimListItem>>>;

public record ClaimListItem(
    Guid Id,
    Guid OrderId,
    string Reason,
    string? ImageUrl,
    string Status,
    DateTime CreatedAt
);
