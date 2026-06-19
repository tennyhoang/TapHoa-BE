using Microsoft.Extensions.Options;
using TapHoa.Infrastructure.Payment;

namespace TapHoa.Api.Filters;

public sealed class SepayWebhookSecretFilter(IOptions<SepayOptions> options) : IEndpointFilter
{
    private const string HeaderName = "x-sepay-secret-key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var expectedSecret = options.Value.SecretKey;

        // Misconfigured — refuse all requests rather than silently bypassing auth
        if (string.IsNullOrEmpty(expectedSecret))
            return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);

        var incoming = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();
        if (!string.Equals(incoming, expectedSecret, StringComparison.Ordinal))
            return Results.Unauthorized();

        return await next(context);
    }
}
