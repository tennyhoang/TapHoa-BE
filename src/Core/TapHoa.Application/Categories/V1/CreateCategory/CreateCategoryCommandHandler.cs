using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Categories.V1.CreateCategory;

public class CreateCategoryCommandHandler(
    IRepository<Category> categoryRepo,
    IDistributedCache cache)
    : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue && !await categoryRepo.AnyAsync(c => c.Id == request.ParentId))
            throw new KeyNotFoundException("Danh mục cha không tồn tại.");

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentId = request.ParentId
        };

        await categoryRepo.AddAsync(category);
        await categoryRepo.SaveChangesAsync();

        await cache.RemoveAsync(CacheKeys.CategoriesAll);
        return GetCategories.GetCategoriesQueryHandler.MapToResponse(category);
    }
}
