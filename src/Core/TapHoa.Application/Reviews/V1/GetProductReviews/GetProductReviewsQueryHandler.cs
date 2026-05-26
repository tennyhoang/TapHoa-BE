using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Reviews.V1.GetProductReviews;

public class GetProductReviewsQueryHandler(IRepository<Review> reviewRepo)
    : IRequestHandler<GetProductReviewsQuery, List<ReviewResponse>>
{
    public async Task<List<ReviewResponse>> Handle(GetProductReviewsQuery request, CancellationToken cancellationToken)
    {
        var reviews = await reviewRepo.Query()
            .Include(r => r.User)
            .Where(r => r.ProductId == request.ProductId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return reviews.Select(MapToResponse).ToList();
    }

    public static ReviewResponse MapToResponse(Review r) => new(
        r.Id, r.UserId, r.User.FullName, r.Rating, r.Comment, r.CreatedAt, r.Sentiment);
}
