using MediatR;

namespace TapHoa.Application.Reviews.V1.GetProductReviews;

public record GetProductReviewsQuery(Guid ProductId) : IRequest<List<ReviewResponse>>;
