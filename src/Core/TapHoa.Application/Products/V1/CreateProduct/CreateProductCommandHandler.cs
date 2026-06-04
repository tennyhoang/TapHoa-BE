using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Products.V1.CreateProduct;

public class CreateProductCommandHandler(
    IRepository<Product> productRepo,
    IRepository<Category> categoryRepo,
    IDistributedCache cache)
    : IRequestHandler<CreateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (!await categoryRepo.AnyAsync(c => c.Id == request.CategoryId))
            throw new KeyNotFoundException("Danh mục không tồn tại.");

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountPrice = request.DiscountPrice,
            Stock = request.Stock,
            ThumbnailUrl = request.ThumbnailUrl,
            CategoryId = request.CategoryId,
            Images = request.Images.Select((url, i) => new ProductImage
            {
                ImageUrl = url,
                SortOrder = i
            }).ToList()
        };

        await productRepo.AddAsync(product);
        await productRepo.SaveChangesAsync();

        var created = await productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstAsync(p => p.Id == product.Id, cancellationToken);

        var result = GetProducts.GetProductsQueryHandler.MapToResponse(created);
        await cache.RemoveAsync($"products:single:{product.Id}", cancellationToken);
        return result;
    }
}
