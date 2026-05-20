using MediatR;
using System.Security.Claims;
using TapHoa.Application.Claims.V1.ApproveClaim;
using TapHoa.Application.Claims.V1.CreateClaim;

namespace TapHoa.Api.Endpoints.V1.Claims;

public static class ClaimEndpoints
{
    public static void MapClaimEndpoints(this IEndpointRouteBuilder app)
    {
        // Customer: gửi khiếu nại cho đơn hàng đã nhận
        app.MapPost("/api/v1/customer/orders/{orderId:guid}/claims", async (
            Guid orderId, CreateClaimRequest body, ClaimsPrincipal user, IMediator mediator) =>
        {
            var customerId = Guid.Parse(
                user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await mediator.Send(
                new CreateClaimCommand(orderId, customerId, body.Reason, body.ImageUrl));

            return result.IsSuccess
                ? Results.Created($"/api/v1/customer/orders/{orderId}/claims", result.Value)
                : result.ErrorCode is "ORDER_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : Results.Conflict(new { result.Error, result.ErrorCode });
        }).RequireAuthorization().WithTags("Claims");

        // Admin: duyệt khiếu nại → hoàn tiền đơn hàng
        app.MapPatch("/api/v1/admin/claims/{id:guid}/approve", async (
            Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ApproveClaimCommand(id));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.ErrorCode is "CLAIM_NOT_FOUND"
                    ? Results.NotFound(new { result.Error, result.ErrorCode })
                    : Results.Conflict(new { result.Error, result.ErrorCode });
        }).RequireAuthorization("Admin").WithTags("Claims");
    }
}

public record CreateClaimRequest(string Reason, string? ImageUrl);
