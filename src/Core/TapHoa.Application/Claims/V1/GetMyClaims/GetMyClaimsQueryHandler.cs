using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Claims.V1.GetMyClaims;

public class GetMyClaimsQueryHandler(IRepository<OrderClaim> claimRepo)
    : IRequestHandler<GetMyClaimsQuery, Result<List<ClaimListItem>>>
{
    public async Task<Result<List<ClaimListItem>>> Handle(GetMyClaimsQuery request, CancellationToken ct)
    {
        var claims = await claimRepo.Query()
            .Where(c => c.CustomerId == request.CustomerId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ClaimListItem(c.Id, c.OrderId, c.Reason, c.ImageUrl, c.Status.ToString(), c.CreatedAt))
            .ToListAsync(ct);

        return Result<List<ClaimListItem>>.Ok(claims);
    }
}
