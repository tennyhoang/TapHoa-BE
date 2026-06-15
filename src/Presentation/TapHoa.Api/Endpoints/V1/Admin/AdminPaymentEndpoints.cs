using MediatR;
using TapHoa.Application.Payment.V1.ProcessRefund;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminPaymentEndpoints
{
    public static void MapAdminPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/payments")
            .RequireAuthorization("Admin");

        group.MapPost("/{orderId:guid}/refund", async (Guid orderId, ProcessRefundRequest? body, IMediator mediator) =>
        {
            var result = await mediator.Send(new ProcessRefundCommand(orderId, body?.Reason));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        });
    }
}

public record ProcessRefundRequest(string? Reason);