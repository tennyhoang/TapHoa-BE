using MediatR;
using System.Security.Claims;
using TapHoa.Application.UserHubs.V1.AddFavoriteHub;
using TapHoa.Application.UserHubs.V1.GetFavoriteHubs;
using TapHoa.Application.UserHubs.V1.RemoveFavoriteHub;
using TapHoa.Application.Users.V1.Admin;
using TapHoa.Application.Users.V1.ChangePassword;
using TapHoa.Application.Users.V1.GetMyProfile;
using TapHoa.Application.Users.V1.UpdateMyProfile;

namespace TapHoa.Api.Endpoints.V1.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization();

        // ── Admin: list / edit / delete users ─────────────────────────────────
        group.MapGet("/", async (IMediator mediator,
            int page = 1, int pageSize = 20, string? search = null, string? role = null) =>
        {
            var result = await mediator.Send(new GetAllUsersQuery(page, pageSize, search, role));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(
                new UpdateUserCommand(id, body.FullName, body.PhoneNumber, body.Role, body.IsActive, body.AgentHubId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
        {
            var requesterId = GetUserId(user);
            var result = await mediator.Send(new DeleteUserCommand(id, requesterId));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapGet("/me", async (ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetMyProfileQuery(GetUserId(user)))));

        group.MapPut("/me", async (UpdateMyProfileRequest request, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(
                new UpdateMyProfileCommand(GetUserId(user), request.FullName, request.PhoneNumber, request.AvatarUrl))));

        group.MapPatch("/me/password", async (ChangePasswordRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            await mediator.Send(new ChangePasswordCommand(GetUserId(user), request.CurrentPassword, request.NewPassword));
            return Results.NoContent();
        });

        // Favorite Hubs
        group.MapGet("/me/favorite-hubs", async (ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetFavoriteHubsQuery(GetUserId(user)))));

        group.MapPost("/me/favorite-hubs/{hubId:guid}", async (Guid hubId, ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new AddFavoriteHubCommand(GetUserId(user), hubId));
            return result.IsSuccess
                ? Results.Created($"/api/v1/users/me/favorite-hubs", result.Value)
                : result.ErrorCode is "HUB_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : Results.Conflict(new { result.Error, result.ErrorCode });
        });

        group.MapDelete("/me/favorite-hubs/{hubId:guid}", async (Guid hubId, ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveFavoriteHubCommand(GetUserId(user), hubId));
            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(new { result.Error, result.ErrorCode });
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record UpdateMyProfileRequest(string FullName, string? PhoneNumber, string? AvatarUrl);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateUserRequest(string? FullName, string? PhoneNumber, string? Role, bool? IsActive, Guid? AgentHubId);
