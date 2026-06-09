using MediatR;
using System.Security.Claims;
using TapHoa.Application.Notifications.V1.GetNotifications;
using TapHoa.Application.Notifications.V1.GetUnreadNotificationCount;
using TapHoa.Application.Notifications.V1.MarkAllNotificationsRead;
using TapHoa.Application.Notifications.V1.MarkNotificationRead;

namespace TapHoa.Api.Endpoints.V1.Notifications;

public static class NotificationEndpoints
{
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/notifications")
            .WithTags("Notifications")
            .RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal user, IMediator mediator, int page = 1, int pageSize = 20) =>
        {
            var userId = GetUserId(user);
            return Results.Ok(await mediator.Send(new GetNotificationsQuery(userId, page, pageSize)));
        });

        group.MapGet("/unread-count", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = GetUserId(user);
            var count = await mediator.Send(new GetUnreadNotificationCountQuery(userId));
            return Results.Ok(new { count });
        });

        group.MapPatch("/{id:guid}/read", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = GetUserId(user);
            var ok = await mediator.Send(new MarkNotificationReadCommand(id, userId));
            return ok ? Results.NoContent() : Results.NotFound();
        });

        group.MapPatch("/read-all", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = GetUserId(user);
            await mediator.Send(new MarkAllNotificationsReadCommand(userId));
            return Results.NoContent();
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
