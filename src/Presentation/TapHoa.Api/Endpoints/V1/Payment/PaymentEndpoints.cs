using MediatR;
using Microsoft.Extensions.Options;
using TapHoa.Api.Filters;
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
            // Lớp 1 (SepayWebhookSecretFilter): x-sepay-secret-key đã được kiểm tra trước khi vào đây.
            // Lớp 2: Authorization: Apikey {key} do SePay gửi kèm mỗi request.
            var authHeader = req.Headers.Authorization.FirstOrDefault() ?? string.Empty;
            var expectedKey = $"Apikey {options.Value.ApiKey}";
            if (!string.Equals(authHeader, expectedKey, StringComparison.Ordinal))
                return Results.Unauthorized();

            var matched = await mediator.Send(new ConfirmPaymentCommand(payload));
            return Results.Ok(new { success = true, matched });
        })
        .AllowAnonymous()
        .AddEndpointFilter<SepayWebhookSecretFilter>();
    }
}
