using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using TapHoa.Infrastructure.Moderation;

namespace TapHoa.Infrastructure.Tests.Moderation;

public class GroqModerationServiceTests
{
    [Fact]
    public async Task ModerateAsync_CleanContent_ReturnsNonToxicPositive()
    {
        var (handler, config) = CreateMocks(HttpStatusCode.OK, BuildGroqResponse(false, "Positive"));
        var service = CreateService(handler, config);

        var result = await service.ModerateAsync("Sản phẩm rất tốt!");

        result.IsToxic.Should().BeFalse();
        result.Sentiment.Should().Be("Positive");
    }

    [Fact]
    public async Task ModerateAsync_ToxicContent_ReturnsToxicNegative()
    {
        var (handler, config) = CreateMocks(HttpStatusCode.OK, BuildGroqResponse(true, "Negative"));
        var service = CreateService(handler, config);

        var result = await service.ModerateAsync("Đồ chó, hàng kém quá!");

        result.IsToxic.Should().BeTrue();
        result.Sentiment.Should().Be("Negative");
    }

    [Fact]
    public async Task ModerateAsync_ApiError_ReturnsSafeNeutral()
    {
        var (handler, config) = CreateMocks(HttpStatusCode.InternalServerError, "{}");
        var service = CreateService(handler, config);

        var result = await service.ModerateAsync("test");

        result.IsToxic.Should().BeFalse();
        result.Sentiment.Should().Be("Neutral");
    }

    [Fact]
    public async Task ModerateAsync_EmptyContent_ReturnsSafeNeutral()
    {
        var config = new ConfigurationBuilder().Build();
        var handlerMock = new Mock<HttpMessageHandler>();
        var service = CreateService(handlerMock, config);

        var result = await service.ModerateAsync("");

        result.IsToxic.Should().BeFalse();
        result.Sentiment.Should().Be("Neutral");
    }

    [Fact]
    public async Task ModerateAsync_MissingApiKey_ReturnsSafeNeutral()
    {
        var config = new ConfigurationBuilder().Build();
        var handlerMock = new Mock<HttpMessageHandler>();
        var service = CreateService(handlerMock, config);

        var result = await service.ModerateAsync("test content");

        result.IsToxic.Should().BeFalse();
        result.Sentiment.Should().Be("Neutral");
    }

    [Fact]
    public async Task ModerateAsync_InvalidJsonResponse_ReturnsSafeNeutral()
    {
        var (handler, config) = CreateMocks(HttpStatusCode.OK, "not-json");
        var service = CreateService(handler, config);

        var result = await service.ModerateAsync("test");

        result.IsToxic.Should().BeFalse();
        result.Sentiment.Should().Be("Neutral");
    }

    private static string BuildGroqResponse(bool isToxic, string sentiment)
    {
        var innerContent = JsonSerializer.Serialize(new { isToxic, sentiment });
        var response = new
        {
            choices = new[] { new { message = new { content = innerContent } } },
        };
        return JsonSerializer.Serialize(response);
    }

    private static (Mock<HttpMessageHandler> Handler, IConfiguration Config) CreateMocks(
        HttpStatusCode statusCode, string responseContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(responseContent),
            });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Groq:ApiKey"] = "test-api-key",
            })
            .Build();

        return (handlerMock, config);
    }

    private static GroqModerationService CreateService(
        Mock<HttpMessageHandler> handlerMock, IConfiguration config)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient("groq"))
            .Returns(new HttpClient(handlerMock.Object));

        return new GroqModerationService(httpClientFactoryMock.Object, config);
    }
}
