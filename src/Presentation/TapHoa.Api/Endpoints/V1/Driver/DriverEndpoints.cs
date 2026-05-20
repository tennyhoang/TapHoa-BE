using MediatR;
using System.Security.Claims;
using TapHoa.Application.Driver.V1;

namespace TapHoa.Api.Endpoints.V1.Driver;

public static class DriverEndpoints
{
    public static void MapDriverEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/driver").WithTags("Driver")
            .RequireAuthorization("Driver");

        // Nhận hàng từ kho, chuyển trạng thái Confirmed → Shipping (batch)
        group.MapPatch("/orders/pickup-from-warehouse", async (
            PickupRequest body, ClaimsPrincipal user, IMediator mediator) =>
        {
            var driverId = Guid.Parse(
                user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await mediator.Send(
                new DriverPickupFromWarehouseCommand(driverId, body.OrderIds));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });
    }
}

public record PickupRequest(List<Guid> OrderIds);
