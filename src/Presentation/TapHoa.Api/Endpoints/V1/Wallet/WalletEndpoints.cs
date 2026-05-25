using MediatR;
using System.Security.Claims;
using TapHoa.Application.Wallet.V1.GetWallet;
using TapHoa.Application.Wallet.V1.GetWalletTransactions;

namespace TapHoa.Api.Endpoints.V1.Wallet;

public static class WalletEndpoints
{
    public static void MapWalletEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/wallet")
            .WithTags("Wallet")
            .RequireAuthorization();

        group.MapGet("/me", async (ClaimsPrincipal principal, IMediator mediator) =>
        {
            var userId = GetUserId(principal);
            var result = await mediator.Send(new GetWalletQuery(userId));
            return Results.Ok(result);
        });

        group.MapGet("/me/transactions", async (
            ClaimsPrincipal principal,
            IMediator mediator,
            int page     = 1,
            int pageSize = 20) =>
        {
            var userId = GetUserId(principal);
            var result = await mediator.Send(new GetWalletTransactionsQuery(userId, page, pageSize));
            return Results.Ok(result);
        });
    }

    private static Guid GetUserId(ClaimsPrincipal principal) =>
        Guid.Parse(
            principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
