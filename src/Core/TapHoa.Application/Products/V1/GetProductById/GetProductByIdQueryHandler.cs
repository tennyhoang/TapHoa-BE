using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Products.V1.GetProductById;

public class GetProductByIdQueryHandler(IRepository<Product> productRepo)
    : IRequestHandler<GetProductByIdQuery, ProductResponse>
{
    public async Task<ProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        return GetProducts.GetProductsQueryHandler.MapToResponse(product);
    }
}
