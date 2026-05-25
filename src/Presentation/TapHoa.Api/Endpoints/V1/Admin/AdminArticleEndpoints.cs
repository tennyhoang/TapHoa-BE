using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TapHoa.Api.Endpoints.V1.Admin;

public static class AdminArticleEndpoints
{
    public static void MapAdminArticleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin/articles")
            .WithTags("Admin - Articles")
            .RequireAuthorization("Admin");

        group.MapPost("/generate", async (
            [FromBody] GenerateArticleRequest request,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration) =>
        {
            var apiKey = configuration["Groq:ApiKey"]
                ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                return Results.BadRequest(new { error = "GROQ_API_KEY chưa được cấu hình" });

            var prompt = $$"""
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

            var body = JsonSerializer.Serialize(new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            });

            try
            {
                var client = httpClientFactory.CreateClient("groq");
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

                var httpResponse = await client.PostAsync(
                    "https://api.groq.com/openai/v1/chat/completions",
                    new StringContent(body, Encoding.UTF8, "application/json"));

                var json = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                    return Results.BadRequest(new { error = $"Groq API lỗi: {httpResponse.StatusCode}", detail = json });

                using var doc = JsonDocument.Parse(json);

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
                return Results.Ok(articleDoc.RootElement.Clone());
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        });
    }
}

public record GenerateArticleRequest(string Topic, string Category);
