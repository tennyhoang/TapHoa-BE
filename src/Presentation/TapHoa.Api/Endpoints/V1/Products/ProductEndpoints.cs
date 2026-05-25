using MediatR;
using TapHoa.Application.Contracts;
using TapHoa.Application.Products.V1.CreateProduct;
using TapHoa.Application.Products.V1.DeleteProduct;
using TapHoa.Application.Products.V1.GetProductById;
using TapHoa.Application.Products.V1.GetProducts;
using TapHoa.Application.Products.V1.UpdateProduct;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Api.Endpoints.V1.Products;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/products").WithTags("Products");

        group.MapGet("/", async (
            string? search, Guid? categoryId,
            decimal? minPrice, decimal? maxPrice,
            string? sortBy, Guid? hubId,
            bool? isNew, bool? isDiscount,
            int page = 1, int pageSize = 20,
            IMediator mediator = default!) =>
            Results.Ok(await mediator.Send(new GetProductsQuery(
                search, categoryId, minPrice, maxPrice, sortBy ?? "newest", page, pageSize, hubId, isNew, isDiscount))));

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            Results.Ok(await mediator.Send(new GetProductByIdQuery(id))));

        group.MapPost("/", async (CreateProductCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return Results.Created($"/api/v1/products/{result.Id}", result);
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:guid}", async (Guid id, UpdateProductCommand command, IMediator mediator) =>
            Results.Ok(await mediator.Send(command with { Id = id }))
        ).RequireAuthorization("Admin");

        group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            await mediator.Send(new DeleteProductCommand(id));
            return Results.NoContent();
        }).RequireAuthorization("Admin");

        // POST /api/v1/products/upload-image
        // Admin upload ảnh lên Cloudinary, gán URL vào product
        group.MapPost("/{id:guid}/upload-image", async (
            Guid id,
            IFormFile file,
            ICloudinaryService cloudinary,
            IRepository<Product> productRepo) =>
        {
            var product = await productRepo.GetByIdAsync(id);

            if (product is null)
                return Results.NotFound(new { Error = "Không tìm thấy sản phẩm." });

            string url;
            try
            {
                await using var stream = file.OpenReadStream();
                url = await cloudinary.UploadImageAsync(stream, file.FileName, "taphoa_products");
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }

            product.ThumbnailUrl = url;
            await productRepo.SaveChangesAsync();

            return Results.Ok(new { ThumbnailUrl = url });
        })
        .RequireAuthorization("Admin")
        .DisableAntiforgery();
    }
}
