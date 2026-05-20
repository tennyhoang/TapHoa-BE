using MediatR;
using TapHoa.Application.Logistics.V1.GetPickingList;
using TapHoa.Domain.Enums;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminLogisticsEndpoints
{
    public static void MapAdminLogisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/logistics").WithTags("Admin - Logistics")
            .RequireAuthorization("Admin");

        // Danh sách hàng cần gom cho ngày cụ thể (mặc định: hôm nay, trạng thái: Confirmed)
        group.MapGet("/picking-list", async (
            IMediator mediator,
            OrderStatus status = OrderStatus.Confirmed,
            DateOnly? date = null) =>
            Results.Ok(await mediator.Send(new GetPickingListQuery(status, date))));
    }
}
