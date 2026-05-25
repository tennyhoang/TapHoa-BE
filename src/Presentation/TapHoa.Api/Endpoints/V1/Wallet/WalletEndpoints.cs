using MediatR;
using System.Security.Claims;
using TapHoa.Application.Wallet.V1.GetWallet;
using TapHoa.Application.Wallet.V1.GetWalletTransactions;
using TapHoa.Application.Wallet.V1.TopupWallet;
using TapHoa.Application.Wallet.V1.WithdrawWallet;

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

        group.MapPost("/me/topup", async (
            ClaimsPrincipal principal,
            IMediator mediator,
            WalletAmountRequest body) =>
        {
            if (body.Amount <= 0)
                return Results.BadRequest(new { message = "Số tiền phải lớn hơn 0." });
            if (body.Amount > 50_000_000)
                return Results.BadRequest(new { message = "Số tiền nạp tối đa là 50.000.000đ." });

            var userId = GetUserId(principal);
            var result = await mediator.Send(new TopupWalletCommand(userId, body.Amount));
            return Results.Ok(result);
        });

        group.MapPost("/me/withdraw", async (
            ClaimsPrincipal principal,
            IMediator mediator,
            WalletAmountRequest body) =>
        {
            if (body.Amount <= 0)
                return Results.BadRequest(new { message = "Số tiền phải lớn hơn 0." });

            var userId = GetUserId(principal);
            var result = await mediator.Send(new WithdrawWalletCommand(userId, body.Amount));
            return Results.Ok(result);
        });
    }

    private static Guid GetUserId(ClaimsPrincipal principal) =>
        Guid.Parse(
            principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record WalletAmountRequest(decimal Amount);
