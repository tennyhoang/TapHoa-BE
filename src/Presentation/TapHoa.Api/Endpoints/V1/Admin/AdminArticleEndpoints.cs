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
            var apiKey = configuration["Gemini:ApiKey"]
                ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY")
                ?? Environment.GetEnvironmentVariable("Gemini__ApiKey");
            if (string.IsNullOrEmpty(apiKey))
                return Results.BadRequest(new { error = "GEMINI_API_KEY chưa được cấu hình" });

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
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            });

            var client = httpClientFactory.CreateClient("gemini");
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            var httpResponse = await client.PostAsync(
                url,
                new StringContent(body, Encoding.UTF8, "application/json"));

            if (!httpResponse.IsSuccessStatusCode)
                return Results.BadRequest(new { error = $"Gemini API lỗi: {httpResponse.StatusCode}" });

            var json = await httpResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "";

            var jsonStart = text.IndexOf('{');
            var jsonEnd   = text.LastIndexOf('}');
            if (jsonStart < 0 || jsonEnd < 0)
                return Results.BadRequest(new { error = "Gemini trả về format không hợp lệ, thử lại" });

            using var articleDoc = JsonDocument.Parse(text[jsonStart..(jsonEnd + 1)]);
            return Results.Ok(articleDoc.RootElement.Clone());
        });
    }
}

public record GenerateArticleRequest(string Topic, string Category);
