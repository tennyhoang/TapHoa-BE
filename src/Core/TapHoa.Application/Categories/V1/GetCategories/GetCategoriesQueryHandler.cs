using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Categories.V1.GetCategories;

public class GetCategoriesQueryHandler(
    IRepository<Category> categoryRepo,
    IDistributedCache cache)
    : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var cached = await CacheHelper.GetAsync<List<CategoryResponse>>(cache, CacheKeys.CategoriesAll, cancellationToken);
        if (cached is not null)
            return cached;

        var categories = await categoryRepo.Query()
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .ToListAsync(cancellationToken);

        var result = categories.Select(MapToResponse).ToList();
        await CacheHelper.SetAsync(cache, CacheKeys.CategoriesAll, result, CacheTtl, cancellationToken);
        return result;
    }

    internal static CategoryResponse MapToResponse(Category c) => new(
        c.Id, c.Name, c.Description, c.ImageUrl, c.ParentId,
        c.Children.Select(MapToResponse).ToList()
    );
}
