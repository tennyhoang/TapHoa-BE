using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Products.V1.GetProductById;

public class GetProductByIdQueryHandler(
    IRepository<Product> productRepo,
    IDistributedCache cache,
    ICacheHelper cacheHelper)
    : IRequestHandler<GetProductByIdQuery, ProductResponse>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(120);

    public async Task<ProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:single:{request.Id}";
        var cached = await cacheHelper.GetAsync<ProductResponse>(cache, cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var product = await productRepo.Query()
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        var result = GetProducts.GetProductsQueryHandler.MapToResponse(product);
        await cacheHelper.SetAsync(cache, cacheKey, result, CacheTtl, cancellationToken);
        return result;
    }
}
