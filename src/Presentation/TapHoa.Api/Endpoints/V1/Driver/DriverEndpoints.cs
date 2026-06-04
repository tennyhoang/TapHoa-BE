using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TapHoa.Application.Driver.V1;
using TapHoa.Application.Driver.V1.GetDriverWarehouse;
using TapHoa.Application.Driver.V1.OptimizeRoute;

namespace TapHoa.Api.Endpoints.V1.Driver;


public static class DriverEndpoints
{
    public static void MapDriverEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/driver").WithTags("Driver")
            .RequireAuthorization("Driver");

        group.MapGet("/me/warehouse", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDriverWarehouseQuery());
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { result.Error, result.ErrorCode });
        });

        group.MapGet("/active-orders", async (ClaimsPrincipal user, IMediator mediator) =>
        {
            if (!TryGetHubId(user, out var hubId))
                return Results.Forbid();

            var result = await mediator.Send(new GetDriverActiveOrdersQuery(hubId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

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

        group.MapGet("/orders", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDriverOrdersQuery());
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        group.MapGet("/orders/shipping", async (IMediator mediator, int page = 1, int pageSize = 50) =>
        {
            var result = await mediator.Send(new GetDriverShippingOrdersQuery(page, pageSize));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        group.MapGet("/orders/delivered", async (IMediator mediator, int page = 1, int pageSize = 30) =>
        {
            var result = await mediator.Send(new GetDriverDeliveredOrdersQuery(page, pageSize));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        // POST /api/v1/driver/optimize-route
        // Tối ưu lộ trình giao hàng qua OpenRouteService.
        // Kho xuất phát được lấy tự động từ WarehouseId trên tài khoản Driver đang đăng nhập.
        // Input: danh sách địa chỉ Hub cần ghé.
        // Output: danh sách điểm ghé theo thứ tự tối ưu (fallback = thứ tự gốc nếu ORS lỗi).
        group.MapPost("/optimize-route", async (
            [FromBody] OptimizeRouteRequest body,
            ClaimsPrincipal user,
            IMediator mediator) =>
        {
            var driverId = GetUserId(user);
            var result = await mediator.Send(
                new OptimizeRouteCommand(driverId, body.OrderAddresses));

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
public record OptimizeRouteRequest(List<string> OrderAddresses);
