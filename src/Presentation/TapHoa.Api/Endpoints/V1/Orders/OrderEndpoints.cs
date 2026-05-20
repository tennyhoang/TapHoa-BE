using System.Security.Claims;
using MediatR;
using TapHoa.Application.Orders.V1.CancelOrder;
using TapHoa.Application.Orders.V1.CreateOrder;
using TapHoa.Application.Orders.V1.GetAllOrders;
using TapHoa.Application.Orders.V1.GetMyOrders;
using TapHoa.Application.Orders.V1.GetOrderById;
using TapHoa.Application.Orders.V1.UpdateOrderStatus;
using TapHoa.Domain.Enums;

namespace TapHoa.Api.Endpoints.V1.Orders;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/orders").WithTags("Orders").RequireAuthorization();

        group.MapGet("/", async (
            ClaimsPrincipal user, IMediator mediator,
            int page = 1, int pageSize = 10, OrderStatus? status = null) =>
            Results.Ok(await mediator.Send(new GetMyOrdersQuery(GetUserId(user), page, pageSize, status))));

        group.MapGet("/my", async (
            ClaimsPrincipal user, IMediator mediator,
            int page = 1, int pageSize = 10, OrderStatus? status = null,
            DateTimeOffset? dateFrom = null, DateTimeOffset? dateTo = null,
            string? sortByAmount = null) =>
        {
            var result = await mediator.Send(
                new GetMyOrdersQuery(GetUserId(user), page, pageSize, status, dateFrom, dateTo, sortByAmount));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });

        group.MapGet("/all", async (
            IMediator mediator,
            int page = 1, int pageSize = 10, OrderStatus? status = null, string? search = null) =>
        {
            var result = await mediator.Send(new GetAllOrdersQuery(page, pageSize, status, search));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapGet("/{id:guid}", async (Guid id, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetOrderByIdQuery(id, GetUserId(user)))));

        group.MapPost("/", async (CreateOrderRequest body, ClaimsPrincipal user, IMediator mediator) =>
        {
            var result = await mediator.Send(new CreateOrderCommand(GetUserId(user), body.HubId, body.Note));
            return Results.Created($"/api/v1/orders/{result.Id}", result);
        });

        group.MapPatch("/{id:guid}/cancel", async (Guid id, CancelOrderRequest? body, ClaimsPrincipal user, IMediator mediator) =>
            Results.Ok(await mediator.Send(new CancelOrderCommand(id, GetUserId(user), body?.CancelReason))));

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateStatusRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateOrderStatusCommand(id, body.Status, body.CancelReason));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}

public record CreateOrderRequest(Guid HubId, string? Note);
public record CancelOrderRequest(string? CancelReason);
public record UpdateStatusRequest(OrderStatus Status, string? CancelReason = null);
