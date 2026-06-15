using System.Security.Claims;
using MediatR;
using TapHoa.Application.Payment.V1.CreateVnpayPayment;
using TapHoa.Application.Payment.V1.VnpayIpn;

namespace TapHoa.Api.Endpoints.V1.Payment;

public static class VnpayPaymentEndpoints
{
    public static void MapVnpayPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payment/vnpay").WithTags("VNPay Payment");

        group.MapPost("/create", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            // OrderId passed in the request body
            return Results.Ok();
        })
        .RequireAuthorization();

        group.MapPost("/create/{orderId:guid}", async (Guid orderId, ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(new CreateVnpayPaymentCommand(orderId, userId));
            return Results.Ok(result);
        })
        .RequireAuthorization();

        group.MapGet("/ipn", async (HttpRequest request, IMediator mediator) =>
        {
            var parameters = request.Query
                .SelectMany(kvp => kvp.Value, (kvp, v) => new { kvp.Key, Value = v })
                .ToDictionary(x => x.Key, x => x.Value);

            var success = await mediator.Send(new VnpayIpnCommand(parameters));
            return success
                ? Results.Ok(new { RspCode = "00", Message = "Confirm Success" })
                : Results.Ok(new { RspCode = "99", Message = "Confirm Fail" });
        })
        .AllowAnonymous();
    }
}
