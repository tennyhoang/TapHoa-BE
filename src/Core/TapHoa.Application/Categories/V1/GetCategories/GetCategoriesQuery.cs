using MediatR;

namespace TapHoa.Application.Categories.V1.GetCategories;

public record GetCategoriesQuery : IRequest<List<CategoryResponse>>;
