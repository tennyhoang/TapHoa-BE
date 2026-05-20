using MediatR;

namespace TapHoa.Application.Categories.V1.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl
) : IRequest<CategoryResponse>;
