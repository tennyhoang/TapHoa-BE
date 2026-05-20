using MediatR;
using System.Security.Claims;
using TapHoa.Application.Agent.V1;

namespace TapHoa.Api.Endpoints.V1.Agent;

public static class AgentEndpoints
{
    public static void MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/agent").WithTags("Agent")
            .RequireAuthorization("Agent");

        // Xác nhận xe tải đã giao hàng đến trạm (Shipping → ArrivedAtHub)
        group.MapPatch("/orders/{id:guid}/arrive", async (
            Guid id, ClaimsPrincipal user, IMediator mediator) =>
        {
            if (!TryGetAgentHubId(user, out var hubId))
                return Results.Forbid();

            var result = await mediator.Send(new AgentArriveCommand(id, GetUserId(user), hubId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode is "ORDER_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : result.ErrorCode is "HUB_FORBIDDEN"
                        ? Results.Forbid()
                        : Results.Conflict(new { result.Error, result.ErrorCode });
        });

        // Xác nhận đã giao hàng tận tay khách ra lấy (ArrivedAtHub → Delivered)
        group.MapPatch("/orders/{id:guid}/complete-pickup", async (
            Guid id, ClaimsPrincipal user, IMediator mediator) =>
        {
            if (!TryGetAgentHubId(user, out var hubId))
                return Results.Forbid();

            var result = await mediator.Send(new AgentCompletePickupCommand(id, GetUserId(user), hubId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode is "ORDER_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : result.ErrorCode is "HUB_FORBIDDEN"
                        ? Results.Forbid()
                        : Results.Conflict(new { result.Error, result.ErrorCode });
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static bool TryGetAgentHubId(ClaimsPrincipal user, out Guid hubId)
    {
        var raw = user.FindFirstValue("hub_id");
        if (raw is null || !Guid.TryParse(raw, out hubId))
        {
            hubId = Guid.Empty;
            return false;
        }
        return true;
    }
}
