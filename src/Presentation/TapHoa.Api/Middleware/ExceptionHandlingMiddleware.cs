using Microsoft.Extensions.Logging;
using TapHoa.Domain.Exceptions;

namespace TapHoa.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteError(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteError(context, StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (OrderDomainException ex)
        {
            await WriteError(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteError(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteError(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteError(context, StatusCodes.Status500InternalServerError, "Đã xảy ra lỗi, vui lòng thử lại.");
        }
    }

    private static async Task WriteError(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message });
    }
}
