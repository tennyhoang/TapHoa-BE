using MediatR;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Loyalty.V1.GetLoyalty;

public class GetLoyaltyQueryHandler(ILoyaltyRepository loyaltyRepo)
    : IRequestHandler<GetLoyaltyQuery, LoyaltyResponse>
{
    public async Task<LoyaltyResponse> Handle(GetLoyaltyQuery request, CancellationToken cancellationToken)
    {
        var account = await loyaltyRepo.GetAccountAsync(request.UserId, cancellationToken);

        if (account is null)
            return new LoyaltyResponse(0, 0);

        return new LoyaltyResponse(account.PointsBalance, account.TotalEarned);
    }
}
