using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Persistence.Data;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminArticleEndpoints
{
    private const string ImageSystemPrompt =
        """
        You are an expert at writing image prompts for AI image generators.
        Given a Vietnamese food/grocery blog article title and excerpt, write a photorealistic image prompt in English for Flux AI.

        Rules:
        - Always include: professional food photography, natural lighting, vibrant colors, high resolution, clean background
        - If the topic involves comparing quality, choosing freshness, or evaluating products: use a "split image, two panels side by side" composition showing the contrast (e.g., fresh vs stale, good vs bad, organic vs conventional)
        - If the topic is about a specific food item: show that food beautifully plated or displayed at a market
        - Maximum 70 words
        - Return ONLY the prompt text, nothing else, no explanation
        """;

    public static void MapAdminArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/articles")
            .WithTags("Admin - Articles")
            .RequireAuthorization("Admin");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var articles = await db.Articles
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new { a.Id, a.Title, a.Category, a.IsPublished, a.CreatedAt, a.ImageUrl })
                .ToListAsync();
            return Results.Ok(articles);
        });

        group.MapPost("/", async ([FromBody] SaveArticleRequest req, AppDbContext db) =>
        {
            var article = new Article
            {
                Title           = req.Title,
                Excerpt         = req.Excerpt,
                Content         = req.Content,
                Category        = req.Category,
                ImageUrl        = req.ImageUrl,
                ReadTimeMinutes = req.ReadTimeMinutes,
                IsPublished     = true,
            };
            db.Articles.Add(article);
            await db.SaveChangesAsync();
            return Results.Ok(new { article.Id });
        });

        group.MapDelete("/{id:guid}", async (Guid id, AppDbContext db) =>
        {
            var article = await db.Articles.FindAsync(id);
            if (article is null) return Results.NotFound();
            db.Articles.Remove(article);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapPost("/generate", async (
            [FromBody] GenerateArticleRequest request,
            IHttpClientFactory httpClientFactory,
            ICloudinaryService cloudinaryService,
            IConfiguration configuration) =>
        {
            var apiKey = configuration["Groq:ApiKey"]
                ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                return Results.BadRequest(new { error = "GROQ_API_KEY chưa được cấu hình" });

            var articlePrompt = $$"""
                Bạn là chuyên gia dinh dưỡng và thực phẩm Việt Nam. Viết bài blog cho website tạp hóa online TapHoa.

                Chủ đề: "{{request.Topic}}"
                Danh mục: {{request.Category}}

                Yêu cầu:
                - Tiêu đề hấp dẫn, ngắn gọn (dưới 70 ký tự)
                - Mô tả ngắn 1-2 câu (dưới 150 ký tự)
                - Nội dung 400-600 từ, chia 3-4 đoạn với heading markdown (##)
                - Thông tin chính xác, paraphrase từ WHO/FAO/Bộ Y tế Việt Nam, không copy nguyên văn
                - Văn phong gần gũi, thực tế cho người nội trợ Việt Nam
                - Kết thúc bằng 3-5 gợi ý thực tế

                Trả về JSON theo đúng format này, không kèm markdown code block:
                {"title": "...", "excerpt": "...", "content": "..."}
                """;

            var articleBody = JsonSerializer.Serialize(new
            {
                model           = "llama-3.3-70b-versatile",
                messages        = new[] { new { role = "user", content = articlePrompt } },
                temperature     = 0.7,
                response_format = new { type = "json_object" },
            });

            try
            {
                var client = httpClientFactory.CreateClient("groq");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                // ── Step 1: generate article text ─────────────────────────────
                var articleResponse = await client.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new StringContent(articleBody, Encoding.UTF8, "application/json"));

                var articleJson = await articleResponse.Content.ReadAsStringAsync();

                if (!articleResponse.IsSuccessStatusCode)
                    return Results.BadRequest(new { error = $"Groq API lỗi: {articleResponse.StatusCode}", detail = articleJson });

                using var doc = JsonDocument.Parse(articleJson);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";

                var jsonStart = text.IndexOf('{');
                var jsonEnd   = text.LastIndexOf('}');
                if (jsonStart < 0 || jsonEnd < 0)
                    return Results.BadRequest(new { error = "Groq trả về format không hợp lệ, thử lại", raw = text });

                using var articleDoc = JsonDocument.Parse(text[jsonStart..(jsonEnd + 1)]);
                var root = articleDoc.RootElement;

                var title   = root.TryGetProperty("title",   out var t) ? t.GetString() ?? "" : "";
                var excerpt = root.TryGetProperty("excerpt", out var e) ? e.GetString() ?? "" : "";
                var content = root.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";

                // ── Step 2: generate image prompt via Groq ────────────────────
                string? imageUrl = null;
                try
                {
                    var imagePromptBody = JsonSerializer.Serialize(new
                    {
                        model       = "llama-3.3-70b-versatile",
                        messages    = new[]
                        {
                            new { role = "system", content = ImageSystemPrompt },
                            new { role = "user",   content = $"Title: {title}\nExcerpt: {excerpt}" },
                        },
                        temperature = 0.8,
                        max_tokens  = 150,
                    });

                    var imagePromptResponse = await client.PostAsync(
                        "https://api.groq.com/openai/v1/chat/completions",
                        new StringContent(imagePromptBody, Encoding.UTF8, "application/json"));

                    if (imagePromptResponse.IsSuccessStatusCode)
                    {
                        var ipJson = await imagePromptResponse.Content.ReadAsStringAsync();
                        using var ipDoc = JsonDocument.Parse(ipJson);
                        var imagePrompt = ipDoc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString()?.Trim() ?? "";

                        // ── Step 3: generate via Pollinations.ai, upload stream to Cloudinary ──
                        if (!string.IsNullOrWhiteSpace(imagePrompt))
                        {
                            var seed          = Math.Abs(imagePrompt.GetHashCode() % 1_000_000);
                            var encodedPrompt = Uri.EscapeDataString(imagePrompt);
                            var pollinationsUrl =
                                $"https://image.pollinations.ai/prompt/{encodedPrompt}" +
                                $"?width=1200&height=630&model=flux-realism&seed={seed}&nologo=true";

                            // Fetch with long timeout — Pollinations needs 30–60 s to generate
                            var pollinationsClient = httpClientFactory.CreateClient("pollinations");
                            using var imageResponse = await pollinationsClient.GetAsync(
                                pollinationsUrl, HttpCompletionOption.ResponseHeadersRead);

                            if (imageResponse.IsSuccessStatusCode)
                            {
                                await using var imageStream = await imageResponse.Content.ReadAsStreamAsync();
                                imageUrl = await cloudinaryService.UploadImageAsync(
                                    imageStream, "article.jpg", "taphoa_articles");
                            }
                        }
                    }
                }
                catch
                {
                    // Image generation is best-effort — never fail the whole request
                }

                return Results.Ok(new { title, excerpt, content, imageUrl });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });
    }
}

public record GenerateArticleRequest(string Topic, string Category);
public record SaveArticleRequest(
    string Title,
    string Excerpt,
    string Content,
    string Category,
    string? ImageUrl,
    int ReadTimeMinutes = 5);
