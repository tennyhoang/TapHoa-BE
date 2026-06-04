using System.Diagnostics;
using System.Security.Claims;

namespace TapHoa.Api.Middleware;

public class AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
{
    private static readonly HashSet<string> _auditMethods = ["POST", "PUT", "PATCH", "DELETE"];
    private static readonly HashSet<string> _skipPaths   = ["/health", "/storage", "/favicon.ico"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path   = context.Request.Path.Value ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        if (!_auditMethods.Contains(method) || _skipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var sw     = Stopwatch.StartNew();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";
        var ip     = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        await next(context);
        sw.Stop();

        logger.LogInformation(
            "[AUDIT] {Method} {Path} | User={UserId} | IP={IP} | Status={Status} | Duration={DurationMs}ms",
            method, path, userId, ip, context.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}
