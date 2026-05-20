using MediatR;
using TapHoa.Application.Admin.V1.GetRevenueStats;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminRevenueEndpoints
{
    public static void MapAdminRevenueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/revenue")
            .WithTags("Admin - Revenue")
            .RequireAuthorization("Admin");

        group.MapGet("/stats", async (
            IMediator mediator,
            TimeRange timeRange = TimeRange.Month,
            DateOnly? from = null,
            DateOnly? to = null) =>
        {
            var result = await mediator.Send(new GetRevenueStatsQuery(timeRange, from, to));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });
    }
}
