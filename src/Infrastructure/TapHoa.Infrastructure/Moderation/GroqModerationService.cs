using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TapHoa.Application.Contracts;

namespace TapHoa.Infrastructure.Moderation;

public class GroqModerationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : IReviewModerationService
{
    private const string SystemPrompt =
        """
        Bạn là trợ lý kiểm duyệt đánh giá sản phẩm thương mại điện tử Việt Nam.
        Phân tích nội dung đánh giá và trả về JSON với đúng 2 trường:
        - "isToxic": boolean — true nếu nội dung chứa từ ngữ thô tục, chửi bới, xúc phạm cá nhân, spam link quảng cáo hoặc nội dung phá hoại; false nếu không
        - "sentiment": string — "Positive" nếu người dùng hài lòng/khen ngợi, "Negative" nếu không hài lòng/chê, "Neutral" nếu trung tính hoặc mô tả khách quan
        Chỉ trả về JSON hợp lệ, không thêm giải thích hay markdown.
        """;

    public async Task<ModerationResult> ModerateAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new ModerationResult(false, "Neutral");

        var apiKey = configuration["Groq:ApiKey"]
            ?? Environment.GetEnvironmentVariable("GROQ_API_KEY");

        if (string.IsNullOrEmpty(apiKey))
            return new ModerationResult(false, "Neutral");

        var requestBody = JsonSerializer.Serialize(new
        {
            model           = "llama-3.3-70b-versatile",
            messages        = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content              },
            },
            temperature     = 0.1,
            response_format = new { type = "json_object" },
        });

        try
        {
            var client = httpClientFactory.CreateClient("groq");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var httpResponse = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(requestBody, Encoding.UTF8, "application/json"),
                cancellationToken);

            if (!httpResponse.IsSuccessStatusCode)
                return new ModerationResult(false, "Neutral");

            var json = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);

            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            using var result = JsonDocument.Parse(text);
            var root = result.RootElement;

            var isToxic = root.TryGetProperty("isToxic", out var toxicEl) && toxicEl.GetBoolean();

            var sentiment = root.TryGetProperty("sentiment", out var sentEl)
                ? sentEl.GetString() ?? "Neutral"
                : "Neutral";

            sentiment = sentiment switch
            {
                "Positive" or "Negative" or "Neutral" => sentiment,
                _ => "Neutral",
            };

            return new ModerationResult(isToxic, sentiment);
        }
        catch
        {
            return new ModerationResult(false, "Neutral");
        }
    }
}
