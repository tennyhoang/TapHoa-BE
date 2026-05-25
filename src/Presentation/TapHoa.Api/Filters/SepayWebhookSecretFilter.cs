using Microsoft.Extensions.Options;
using TapHoa.Infrastructure.Payment;

namespace TapHoa.Api.Filters;

/// <summary>
/// Kiểm tra header x-sepay-secret-key trước khi cho phép request vào webhook endpoint.
/// Trả về 401 ngay lập tức nếu secret không khớp hoặc bị thiếu.
/// Filter không chạy nếu SecretKey chưa được cấu hình (để không block môi trường dev).
/// </summary>
public sealed class SepayWebhookSecretFilter(IOptions<SepayOptions> options) : IEndpointFilter
{
    private const string HeaderName = "x-sepay-secret-key";

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var expectedSecret = options.Value.SecretKey;

        if (!string.IsNullOrEmpty(expectedSecret))
        {
            var incoming = context.HttpContext.Request.Headers[HeaderName].FirstOrDefault();

            if (!string.Equals(incoming, expectedSecret, StringComparison.Ordinal))
                return Results.Unauthorized();
        }

        return await next(context);
    }
}
