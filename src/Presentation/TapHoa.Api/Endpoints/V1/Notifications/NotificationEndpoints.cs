using MediatR;
using System.Security.Claims;
using TapHoa.Application.Notifications.V1.RegisterPushToken;

namespace TapHoa.Api.Endpoints.V1.Notifications;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications").WithTags("Notifications")
            .RequireAuthorization();

        group.MapPost("/push-token", async (
            RegisterPushTokenRequest body,
            ClaimsPrincipal user,
            IMediator mediator) =>
        {
            var userId = Guid.Parse(
                user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(
                new RegisterPushTokenCommand(userId, body.Token, body.Platform));
            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });
    }
}

public record RegisterPushTokenRequest(string Token, string? Platform);
