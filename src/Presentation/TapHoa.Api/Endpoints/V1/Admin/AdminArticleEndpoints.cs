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
        Given a Vietnamese food/grocery blog article title, excerpt, and category, write a photorealistic image prompt in English for Flux AI.

        Category rules (follow strictly):
        - "san-pham-noi-bat" (featured product): show the product PACKAGING/BAG/BOX as the hero shot — clean studio background, soft shadow, product label clearly visible, commercial product photography style
        - "dinh-duong" (nutrition): ingredients or fresh food beautifully arranged, food styling, natural light
        - "mua-sam-thong-minh" (smart shopping): if comparing quality/freshness use "split image, two panels side by side"; otherwise a market/grocery scene
        - "he-thong-hub" (hub system): modern logistics, delivery, or store scene

        General rules:
        - Always include: professional photography, natural lighting, vibrant colors, high resolution
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
                Bạn là biên tập viên cẩm nang thực phẩm cho website tạp hóa online TapHoa — giọng văn như người bạn am hiểu chia sẻ kinh nghiệm thực tế, không phải giáo sư giảng bài.

                Chủ đề: "{{request.Topic}}"
                Danh mục: {{request.Category}}

                Yêu cầu bắt buộc về NỘI DUNG:
                - Tiêu đề hấp dẫn, cụ thể, ngắn gọn (dưới 70 ký tự) — không dùng từ chung chung như "Hướng dẫn", "Giới thiệu"
                - Excerpt 1-2 câu móc nối cảm xúc hoặc nêu vấn đề cụ thể (dưới 150 ký tự)
                - Nội dung 450-600 từ với đúng cấu trúc sau:
                  • Đoạn mở đầu KHÔNG có heading: 2-3 câu hook gây tò mò hoặc nêu điều ít biết về chủ đề
                  • 3-4 section với ## heading ngắn gọn, súc tích (VD: "## Chọn đúng — tránh hàng giả", "## Bảo quản đúng cách")
                  • Mỗi section dùng **in đậm** cho từ khóa quan trọng, *in nghiêng* cho tên chuyên môn hoặc lưu ý
                  • Ít nhất 2 section có bullet list (- item) với thông tin cụ thể, không chung chung
                  • Section cuối là gợi ý thực tế cho bếp gia đình (3-5 gợi ý dạng bullet)

                Yêu cầu về PHONG CÁCH:
                - Xưng "bạn", giọng thân mật như người bạn mách nước, không phải hướng dẫn khô khan
                - Dùng số liệu cụ thể khi có thể (VD: "chứa 3.5mg sắt/100g", "bảo quản được 2-3 tháng")
                - Thông tin chính xác, paraphrase từ Viện Dinh dưỡng Quốc gia / WHO / Bộ Y tế VN
                - KHÔNG dùng các mẫu câu sáo rỗng như: "không chỉ... mà còn", "hãy cùng khám phá", "hy vọng bài viết"

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
                string? imageError = null;
                try
                {
                    var imagePromptBody = JsonSerializer.Serialize(new
                    {
                        model       = "llama-3.3-70b-versatile",
                        messages    = new[]
                        {
                            new { role = "system", content = ImageSystemPrompt },
                            new { role = "user",   content = $"Category: {request.Category}\nTitle: {title}\nExcerpt: {excerpt}" },
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

                        // ── Step 3: generate via Pollinations.ai, upload to Cloudinary ──
                        if (!string.IsNullOrWhiteSpace(imagePrompt))
                        {
                            var seed          = Math.Abs(imagePrompt.GetHashCode() % 1_000_000);
                            var encodedPrompt = Uri.EscapeDataString(imagePrompt);
                            var pollinationsUrl =
                                $"https://image.pollinations.ai/prompt/{encodedPrompt}" +
                                $"?width=1200&height=630&model=flux-realism&seed={seed}&nologo=true";

                            var pollinationsClient = httpClientFactory.CreateClient("pollinations");
                            using var imageResponse = await pollinationsClient.GetAsync(pollinationsUrl);

                            if (imageResponse.IsSuccessStatusCode)
                            {
                                var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
                                using var ms = new MemoryStream(imageBytes);
                                imageUrl = await cloudinaryService.UploadImageAsync(
                                    ms, "article.jpg", "taphoa_articles");
                            }
                            else
                            {
                                imageError = $"Pollinations {(int)imageResponse.StatusCode}";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    imageError = ex.Message;
                }

                return Results.Ok(new { title, excerpt, content, imageUrl, imageError });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }).RequireRateLimiting("AiPolicy");
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
