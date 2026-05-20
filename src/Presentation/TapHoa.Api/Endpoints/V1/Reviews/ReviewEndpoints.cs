using MediatR;
using System.Security.Claims;
using TapHoa.Application.Reviews.V1.AddReview;
using TapHoa.Application.Reviews.V1.GetProductReviews;

namespace TapHoa.Api.Endpoints.V1.Reviews;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products/{productId:guid}/reviews").WithTags("Reviews");

        group.MapGet("/", async (Guid productId, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetProductReviewsQuery(productId))));

        group.MapPost("/", async (Guid productId, AddReviewRequest request, ClaimsPrincipal user, IMediator mediator) =>
        {
            var userId = Guid.Parse(user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await mediator.Send(new AddReviewCommand(productId, userId, request.Rating, request.Comment));
            return Results.Created($"/api/v1/products/{productId}/reviews/{result.Id}", result);
        }).RequireAuthorization();
    }
}

public record AddReviewRequest(int Rating, string? Comment);
