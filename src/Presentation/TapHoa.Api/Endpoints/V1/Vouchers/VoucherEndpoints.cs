using MediatR;
using TapHoa.Application.Vouchers.V1.ValidateVoucher;

namespace TapHoa.Api.Endpoints.V1.Vouchers;

public static class VoucherEndpoints
{
    public static void MapVoucherEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/vouchers").WithTags("Vouchers");

        group.MapPost("/validate", async (ValidateVoucherRequest body, IMediator mediator) =>
        {
            var result = await mediator.Send(new ValidateVoucherCommand(body.Code, body.Total));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(new { result.Error, result.ErrorCode });
        }).RequireAuthorization().RequireRateLimiting("VoucherPolicy");
    }
}

public record ValidateVoucherRequest(string Code, decimal Total);
