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

        // GET /api/v1/driver/active-orders
        // Đơn chờ gom (Paid_WaitingForBatch) tại Hub của Driver — dùng cho màn hình gom đơn 12h đêm
        group.MapGet("/active-orders", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            if (!TryGetHubId(user, out var hubId))
                return Results.Forbid();

            var result = await mediator.Send(new GetDriverActiveOrdersQuery(hubId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        // POST /api/v1/driver/orders/dispatch
        // Driver xác nhận gom xong → chuyển batch sang ShippingToHub
        group.MapPost("/orders/dispatch", async (
            DispatchRequest body, ClaimsPrincipal user, IMediator mediator) =>
        {
            var driverId = GetUserId(user);

            if (body.OrderIds is not { Count: > 0 })
                return Results.BadRequest(new { Error = "Danh sách đơn hàng không được rỗng." });

            var result = await mediator.Send(new DriverDispatchCommand(driverId, body.OrderIds));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        // GET /api/v1/driver/orders  (giữ nguyên — nhóm tất cả hub, dành cho admin/tổng quan)
        group.MapGet("/orders", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDriverOrdersQuery());
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        // PATCH /api/v1/driver/orders/pickup-from-warehouse  (giữ nguyên — endpoint cũ)
        group.MapPatch("/orders/pickup-from-warehouse", async (
            PickupRequest body, ClaimsPrincipal user, IMediator mediator) =>
        {
            var driverId = GetUserId(user);

            var result = await mediator.Send(
                new DriverPickupFromWarehouseCommand(driverId, body.OrderIds));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static bool TryGetHubId(ClaimsPrincipal user, out Guid hubId)
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

public record PickupRequest(List<Guid> OrderIds);
public record DispatchRequest(List<Guid> OrderIds);
