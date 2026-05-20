using MediatR;
using System.Security.Claims;
using TapHoa.Application.UserHubs.V1.AddFavoriteHub;
using TapHoa.Application.UserHubs.V1.GetFavoriteHubs;
using TapHoa.Application.UserHubs.V1.RemoveFavoriteHub;
using TapHoa.Application.Users.V1.ChangePassword;
using TapHoa.Application.Users.V1.GetMyProfile;
using TapHoa.Application.Users.V1.UpdateMyProfile;

namespace TapHoa.Api.Endpoints.V1.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization();

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
