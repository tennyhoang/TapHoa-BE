using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Categories.V1.DeleteCategory;

public class DeleteCategoryCommandHandler(
    IRepository<Category> categoryRepo,
    IRepository<Product> productRepo,
    IDistributedCache cache)
    : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepo.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");

        if (await categoryRepo.AnyAsync(c => c.ParentId == request.Id))
            throw new InvalidOperationException("Không thể xóa danh mục đang có danh mục con.");

        if (await productRepo.AnyAsync(p => p.CategoryId == request.Id))
            throw new InvalidOperationException("Không thể xóa danh mục đang có sản phẩm.");

        categoryRepo.Remove(category);
        await categoryRepo.SaveChangesAsync();

        await cache.RemoveAsync(CacheKeys.CategoriesAll);
    }
}
