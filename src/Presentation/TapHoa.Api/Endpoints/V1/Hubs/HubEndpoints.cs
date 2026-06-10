using MediatR;
using TapHoa.Application.HubInventories.V1.UpdateHubStock;
using TapHoa.Application.Hubs.V1.CreateHub;
using TapHoa.Application.Hubs.V1.DeleteHub;
using TapHoa.Application.Hubs.V1.GetActiveHubs;
using TapHoa.Application.Hubs.V1.GetAllHubs;
using TapHoa.Application.Hubs.V1.UpdateHub;
using TapHoa.Application.Hubs.V1.UpdateHubStatus;

namespace TapHoa.Api.Endpoints.V1.Hubs;

public static class HubEndpoints
{
    public static void MapHubEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/hubs").WithTags("Hubs");

        // Public: khách hàng tìm Hub gần nhà trước khi đặt hàng (BR-009: sort by distance khi có lat/lng)
        group.MapGet("/", async (IMediator mediator, string? city, string? district,
            double? lat, double? lng) =>
            Results.Ok(await mediator.Send(new GetActiveHubsQuery(city, district, lat, lng))));

        group.MapGet("/all", async (
            IMediator mediator,
            int page = 1, int pageSize = 20,
            TapHoa.Domain.Enums.HubStatus? status = null,
            string? city = null, string? district = null, string? search = null) =>
            Results.Ok(await mediator.Send(new GetAllHubsQuery(page, pageSize, status, city, district, search)))
        ).RequireAuthorization("Admin");

        group.MapPost("/", async (CreateHubCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/hubs/{result.Value!.Id}", result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", async (Guid id, UpdateHubRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateHubCommand(
                id, body.Name, body.Address, body.Ward, body.District, body.City,
                body.Latitude, body.Longitude));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateStatusRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateHubStatusCommand(id, body.Status));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        // Admin: xóa Hub (chỉ khi không còn đơn hàng chưa hoàn tất)
        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteHubCommand(id));
            return result.IsSuccess
                ? Results.NoContent()
                : result.ErrorCode == "HUB_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : Results.Conflict(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");

        group.MapPut("/{hubId:guid}/inventory/{productId:guid}", async (
            Guid hubId, Guid productId, UpdateStockRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(new UpdateHubStockCommand(hubId, productId, body.Stock));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode is "HUB_NOT_FOUND" or "PRODUCT_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin");
    }
}

public record UpdateStatusRequest(TapHoa.Domain.Enums.HubStatus Status);
public record UpdateHubRequest(
    string Name, string Address, string Ward, string District, string City,
    double Latitude, double Longitude);
public record UpdateStockRequest(int Stock);
