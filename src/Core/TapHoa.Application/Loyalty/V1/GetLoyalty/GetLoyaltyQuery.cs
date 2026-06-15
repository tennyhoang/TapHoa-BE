using MediatR;

namespace TapHoa.Application.Loyalty.V1.GetLoyalty;

public record GetLoyaltyQuery(Guid UserId) : IRequest<LoyaltyResponse>;

public record LoyaltyResponse(
    int Points,
    int LifetimePoints
);
