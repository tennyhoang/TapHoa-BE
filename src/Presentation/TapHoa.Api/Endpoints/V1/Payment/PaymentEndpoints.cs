using MediatR;
using Microsoft.Extensions.Options;
using TapHoa.Application.Orders.V1.ConfirmPayment;
using TapHoa.Infrastructure.Payment;

namespace TapHoa.Api.Endpoints.V1.Payment;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/payment/webhook/sepay", async (
            HttpRequest req,
            SepayWebhookPayload payload,
            IMediator mediator,
            IOptions<SepayOptions> options) =>
        {
            // SePay gửi API key trong header "Authorization: Apikey {key}"
            var authHeader = req.Headers.Authorization.FirstOrDefault() ?? string.Empty;
            var expectedKey = $"Apikey {options.Value.ApiKey}";
            if (!string.Equals(authHeader, expectedKey, StringComparison.Ordinal))
                return Results.Unauthorized();

            var matched = await mediator.Send(new ConfirmPaymentCommand(payload));
            return Results.Ok(new { success = true, matched });
        }).AllowAnonymous();
    }
}
