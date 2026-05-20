using MediatR;

namespace TapHoa.Application.Reviews.V1.AddReview;

public record AddReviewCommand(
    Guid ProductId,
    Guid UserId,
    int Rating,
    string? Comment
) : IRequest<ReviewResponse>;
