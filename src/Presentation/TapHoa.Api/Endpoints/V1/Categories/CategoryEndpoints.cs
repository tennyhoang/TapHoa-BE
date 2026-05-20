using MediatR;
using TapHoa.Application.Categories.V1.CreateCategory;
using TapHoa.Application.Categories.V1.DeleteCategory;
using TapHoa.Application.Categories.V1.GetCategories;
using TapHoa.Application.Categories.V1.UpdateCategory;

namespace TapHoa.Api.Endpoints.V1.Categories;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories").WithTags("Categories");

        group.MapGet("/", async (IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetCategoriesQuery())));

        group.MapPost("/", async (CreateCategoryCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/categories/{result.Id}", result);
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", async (Guid id, UpdateCategoryRequest request, IMediator mediator) =>
            Results.Ok(await mediator.Send(new UpdateCategoryCommand(id, request.Name, request.Description, request.ImageUrl)))
        ).RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteCategoryCommand(id));
            return Results.NoContent();
        }).RequireAuthorization("Admin");
    }
}

public record UpdateCategoryRequest(string Name, string? Description, string? ImageUrl);
