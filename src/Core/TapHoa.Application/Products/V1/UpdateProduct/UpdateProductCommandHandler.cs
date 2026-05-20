using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Products.V1.UpdateProduct;

public class UpdateProductCommandHandler(
    IRepository<Product> productRepo,
    IRepository<Category> categoryRepo,
    IRepository<ProductImage> imageRepo)
    : IRequestHandler<UpdateProductCommand, ProductResponse>
{
    public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepo.Query()
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        if (!await categoryRepo.AnyAsync(c => c.Id == request.CategoryId))
            throw new KeyNotFoundException("Danh mục không tồn tại.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountPrice = request.DiscountPrice;
        product.Stock = request.Stock;
        product.ThumbnailUrl = request.ThumbnailUrl;
        product.CategoryId = request.CategoryId;

        foreach (var img in product.Images)
            imageRepo.Remove(img);

        product.Images = request.Images.Select((url, i) => new ProductImage
        {
            ImageUrl = url,
            SortOrder = i,
            ProductId = product.Id
        }).ToList();

        productRepo.Update(product);
        await productRepo.SaveChangesAsync();

        var updated = await productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstAsync(p => p.Id == product.Id, cancellationToken);

        return GetProducts.GetProductsQueryHandler.MapToResponse(updated);
    }
}
