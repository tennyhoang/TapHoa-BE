using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Reviews.V1.AddReview;

public class AddReviewCommandHandler(
    IRepository<Review> reviewRepo,
    IRepository<Product> productRepo,
    IRepository<Order> orderRepo,
    IReviewModerationService moderationService)
    : IRequestHandler<AddReviewCommand, ReviewResponse>
{
    public async Task<ReviewResponse> Handle(AddReviewCommand request, CancellationToken cancellationToken)
    {
        if (request.Rating < 1 || request.Rating > 5)
            throw new ArgumentException("Rating phải từ 1 đến 5.");

        var product = await productRepo.GetByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var hasPurchased = await orderRepo.Query()
            .AnyAsync(o => o.UserId == request.UserId
                && o.Status == OrderStatus.Completed
                && o.Items.Any(i => i.ProductId == request.ProductId),
                cancellationToken);

        if (!hasPurchased)
            throw new InvalidOperationException("Bạn chỉ có thể đánh giá sản phẩm đã mua.");

        var alreadyReviewed = await reviewRepo.AnyAsync(
            r => r.UserId == request.UserId && r.ProductId == request.ProductId);

        if (alreadyReviewed)
            throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi.");

        var moderation = await moderationService.ModerateAsync(
            request.Comment ?? string.Empty, cancellationToken);

        var review = new Review
        {
            UserId = request.UserId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            Comment = request.Comment,
            IsApproved = !moderation.IsToxic,
            Sentiment = moderation.Sentiment,
        };

        await reviewRepo.AddAsync(review);
        await reviewRepo.SaveChangesAsync();

        var saved = await reviewRepo.Query()
            .Include(r => r.User)
            .FirstAsync(r => r.Id == review.Id, cancellationToken);

        return GetProductReviews.GetProductReviewsQueryHandler.MapToResponse(saved);
    }
}
