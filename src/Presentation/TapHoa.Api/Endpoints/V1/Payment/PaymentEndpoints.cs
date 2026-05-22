using System.Security.Cryptography;
using System.Text;
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
            IMediator mediator,
            IOptions<SepayOptions> options) =>
        {
            // Đọc raw body để verify chữ ký HMAC-SHA256
            req.EnableBuffering();
            using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
            var rawBody = await reader.ReadToEndAsync();
            req.Body.Position = 0;

            // Verify HMAC-SHA256: SePay gửi chữ ký trong header "secure-hash"
            var signature = req.Headers["secure-hash"].FirstOrDefault() ?? string.Empty;
            if (!VerifyHmac(rawBody, options.Value.SecretKey, signature))
                return Results.Unauthorized();

            var payload = System.Text.Json.JsonSerializer.Deserialize<SepayWebhookPayload>(
                rawBody,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (payload is null) return Results.BadRequest();

            var matched = await mediator.Send(new ConfirmPaymentCommand(payload));
            return Results.Ok(new { success = true, matched });
        }).AllowAnonymous();
    }

    private static bool VerifyHmac(string body, string secretKey, string signature)
    {
        if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(signature))
            return false;

        var keyBytes  = Encoding.UTF8.GetBytes(secretKey);
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var hash      = HMACSHA256.HashData(keyBytes, bodyBytes);
        var expected  = Convert.ToHexString(hash).ToLower();

        return string.Equals(expected, signature.ToLower(), StringComparison.Ordinal);
    }
}
