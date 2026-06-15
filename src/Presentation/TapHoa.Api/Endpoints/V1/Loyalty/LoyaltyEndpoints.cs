using MediatR;
using System.Security.Claims;
using TapHoa.Application.Loyalty.V1.GetLoyalty;
using TapHoa.Application.Loyalty.V1.GetLoyaltyTransactions;

namespace TapHoa.Api.Endpoints.V1.Loyalty;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/loyalty")
            .WithTags("Loyalty")
            .RequireAuthorization();

        group.MapGet("/me", async (ClaimsPrincipal principal, IMediator mediator) =>
        {
            var userId = GetUserId(principal);
            var result = await mediator.Send(new GetLoyaltyQuery(userId));
            return Results.Ok(result);
        });

        group.MapGet("/me/transactions", async (
            ClaimsPrincipal principal,
            IMediator mediator,
            int page     = 1,
            int pageSize = 20) =>
        {
            var userId = GetUserId(principal);
            var result = await mediator.Send(new GetLoyaltyTransactionsQuery(userId, page, pageSize));
            return Results.Ok(result);
        });
    }

    private static Guid GetUserId(ClaimsPrincipal principal) =>
        Guid.Parse(
            principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
