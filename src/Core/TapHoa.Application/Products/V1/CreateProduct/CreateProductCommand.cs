using System.ComponentModel.DataAnnotations;
using MediatR;

namespace TapHoa.Application.Products.V1.CreateProduct;

public record CreateProductCommand(
    [Required, MaxLength(500)] string Name,
    string? Description,
    [Required, Range(0, double.MaxValue)] decimal Price,
    decimal? DiscountPrice,
    [Required, Range(0, int.MaxValue)] int Stock,
    string? ThumbnailUrl,
    List<string> Images,
    [Required] Guid CategoryId
) : IRequest<ProductResponse>;
