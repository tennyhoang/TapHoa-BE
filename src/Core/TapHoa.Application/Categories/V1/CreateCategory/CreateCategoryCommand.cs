using System.ComponentModel.DataAnnotations;
using MediatR;

namespace TapHoa.Application.Categories.V1.CreateCategory;

public record CreateCategoryCommand(
    [Required, MaxLength(200)] string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentId
) : IRequest<CategoryResponse>;
