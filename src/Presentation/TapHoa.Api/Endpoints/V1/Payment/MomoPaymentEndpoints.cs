using System.Security.Claims;
using MediatR;
using TapHoa.Application.Payment.V1.CreateMomoPayment;
using TapHoa.Application.Payment.V1.MomoIpn;

namespace TapHoa.Api.Endpoints.V1.Payment;

public static class MomoPaymentEndpoints
{
    public static void MapMomoPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/payment/momo").WithTags("MoMo Payment");

        group.MapPost("/create/{orderId:guid}", async (Guid orderId, ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(new CreateMomoPaymentCommand(orderId, userId));
            return Results.Ok(result);
        })
        .RequireAuthorization();

        group.MapPost("/ipn", async (HttpRequest request, IMediator mediator) =>
        {
            Dictionary<string, string> parameters;
            if (request.HasFormContentType)
            {
                parameters = request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
            }
            else
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();
                parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(body)
                    ?? new Dictionary<string, string>();
            }

            var success = await mediator.Send(new MomoIpnCommand(parameters));
            return success ? Results.Ok(new { message = "success" }) : Results.Ok(new { message = "fail" });
        })
        .AllowAnonymous();
    }
}
