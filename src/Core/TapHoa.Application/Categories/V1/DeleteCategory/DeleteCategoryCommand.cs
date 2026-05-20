using MediatR;

namespace TapHoa.Application.Categories.V1.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest;
