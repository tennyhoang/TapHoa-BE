namespace TapHoa.Application.Products.V1;

public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    decimal? DiscountPrice,
    int Stock,
    string? ThumbnailUrl,
    Guid CategoryId,
    string CategoryName,
    double AverageRating,
    int ReviewCount,
    List<string> Images,
    DateTime CreatedAt
);
