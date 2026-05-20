namespace TapHoa.Application.Categories.V1;

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentId,
    List<CategoryResponse> Children
);
