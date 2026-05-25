using MediatR;
using System.Security.Claims;
using TapHoa.Application.Wallet.V1.CreateWithdrawRequest;
using TapHoa.Application.Wallet.V1.GetWallet;
using TapHoa.Application.Wallet.V1.GetWalletTransactions;
using TapHoa.Application.Wallet.V1.InitiateTopup;

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

        // Initiate QR-based top-up — returns paymentRef for VietQR display
        group.MapPost("/me/topup/initiate", async (
            ClaimsPrincipal principal,
            IMediator mediator,
            WalletAmountRequest body) =>
        {
            if (body.Amount <= 0)
                return Results.BadRequest(new { message = "Số tiền phải lớn hơn 0." });
            if (body.Amount > 50_000_000)
                return Results.BadRequest(new { message = "Số tiền nạp tối đa là 50.000.000đ." });

            var userId = GetUserId(principal);
            var result = await mediator.Send(new InitiateWalletTopupCommand(userId, body.Amount));
            return Results.Ok(result);
        });

        // Submit withdraw request with bank info
        group.MapPost("/me/withdraw-request", async (
            ClaimsPrincipal principal,
            IMediator mediator,
            WithdrawRequestBody body) =>
        {
            if (body.Amount <= 0)
                return Results.BadRequest(new { message = "Số tiền phải lớn hơn 0." });
            if (string.IsNullOrWhiteSpace(body.BankName))
                return Results.BadRequest(new { message = "Vui lòng chọn ngân hàng." });
            if (string.IsNullOrWhiteSpace(body.AccountNumber))
                return Results.BadRequest(new { message = "Vui lòng nhập số tài khoản." });
            if (string.IsNullOrWhiteSpace(body.HolderName))
                return Results.BadRequest(new { message = "Vui lòng nhập tên chủ tài khoản." });

            var userId = GetUserId(principal);
            var id = await mediator.Send(new CreateWithdrawRequestCommand(
                userId, body.Amount, body.BankName, body.AccountNumber, body.HolderName));
            return Results.Ok(new { id });
        });
    }

    private static Guid GetUserId(ClaimsPrincipal principal) =>
        Guid.Parse(
            principal.FindFirstValue("sub")
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record WalletAmountRequest(decimal Amount);
public record WithdrawRequestBody(decimal Amount, string BankName, string AccountNumber, string HolderName);
