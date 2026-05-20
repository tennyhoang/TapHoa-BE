using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Categories.V1.GetCategories;

public class GetCategoriesQueryHandler(IRepository<Category> categoryRepo)
    : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
{
    public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepo.Query()
            .Include(c => c.Children)
            .Where(c => c.ParentId == null)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToResponse).ToList();
    }

    internal static CategoryResponse MapToResponse(Category c) => new(
        c.Id, c.Name, c.Description, c.ImageUrl, c.ParentId,
        c.Children.Select(MapToResponse).ToList()
    );
}
