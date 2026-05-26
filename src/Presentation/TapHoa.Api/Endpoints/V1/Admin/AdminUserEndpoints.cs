using MediatR;
using TapHoa.Application.Users.V1.Admin.AssignWarehouse;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminUserEndpoints
{
    public static void MapAdminUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/users").WithTags("Admin - Users")
            .RequireAuthorization("Admin");

        // PATCH /api/v1/admin/users/{id}/assign-warehouse
        // Gán hoặc gỡ kho cho một User (Driver hoặc WarehouseManager).
        // Body: { "warehouseId": "<guid>" | null, "targetType": "Driver" | "Manager" }
        // warehouseId = null → gỡ kho khỏi user.
        group.MapPatch("/{id:guid}/assign-warehouse", async (
            Guid                    id,
            AssignWarehouseRequest  body,
            IMediator               mediator) =>
        {
            var result = await mediator.Send(
                new AssignWarehouseCommand(id, body.WarehouseId, body.TargetType));

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });
    }
}

public record AssignWarehouseRequest(Guid? WarehouseId, string TargetType);
