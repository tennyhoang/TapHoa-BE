using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Categories.V1.UpdateCategory;

public class UpdateCategoryCommandHandler(IRepository<Category> categoryRepo)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
{
    public async Task<CategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await categoryRepo.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy danh mục.");

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;

        categoryRepo.Update(category);
        await categoryRepo.SaveChangesAsync();

        return GetCategories.GetCategoriesQueryHandler.MapToResponse(category);
    }
}
