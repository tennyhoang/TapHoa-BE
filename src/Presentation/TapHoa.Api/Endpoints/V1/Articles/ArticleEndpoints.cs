using Microsoft.EntityFrameworkCore;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Articles;

public static class ArticleEndpoints
{
    public static void MapArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/articles").WithTags("Articles");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var articles = await db.Articles
                .Where(a => a.IsPublished)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.Id,
                    a.Title,
                    a.Excerpt,
                    a.Content,
                    a.Category,
                    a.ImageUrl,
                    a.ReadTimeMinutes,
                    a.CreatedAt,
                })
                .ToListAsync();
            return Results.Ok(articles);
        });
    }
}
