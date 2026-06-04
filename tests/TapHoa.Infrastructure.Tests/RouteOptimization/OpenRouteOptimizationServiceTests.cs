using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using TapHoa.Infrastructure.RouteOptimization;

namespace TapHoa.Infrastructure.Tests.RouteOptimization;

public class OpenRouteOptimizationServiceTests
{
    [Fact]
    public async Task GeocodeAsync_Success_ReturnsCoordinates()
    {
        var nominatimResponse = JsonSerializer.Serialize(new[]
        {
            new { lon = "106.6297", lat = "10.8231" },
        });
        var (handler, config) = CreateMocks(
            ("nominatim", HttpStatusCode.OK, nominatimResponse),
            ("ors", HttpStatusCode.OK, "{}"));
        var service = CreateService(handler, config);

        var result = await service.GeocodeAsync("123 Nguyễn Huệ, HCM");

        result.Should().NotBeNull();
        result!.Length.Should().Be(2);
        result[0].Should().BeApproximately(106.6297, 0.0001);
        result[1].Should().BeApproximately(10.8231, 0.0001);
    }

    [Fact]
    public async Task GeocodeAsync_NotFound_ReturnsNull()
    {
        var (handler, config) = CreateMocks(
            ("nominatim", HttpStatusCode.OK, "[]"),
            ("ors", HttpStatusCode.OK, "{}"));
        var service = CreateService(handler, config);

        var result = await service.GeocodeAsync("Nonexistent Address");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GeocodeAsync_ApiError_ReturnsNull()
    {
        var (handler, config) = CreateMocks(
            ("nominatim", HttpStatusCode.InternalServerError, ""),
            ("ors", HttpStatusCode.OK, "{}"));
        var service = CreateService(handler, config);

        var result = await service.GeocodeAsync("123 Street");

        result.Should().BeNull();
    }

    [Fact]
    public async Task OptimizeAsync_Success_ReturnsOptimizedOrder()
    {
        var orsResponse = JsonSerializer.Serialize(new
        {
            routes = new[]
            {
                new
                {
                    steps = new[]
                    {
                        new { type = "job", id = 1 },
                        new { type = "job", id = 0 },
                        new { type = "end", id = -1 },
                    },
                },
            },
        });
        var (handler, config) = CreateMocks(
            ("nominatim", HttpStatusCode.OK, "[]"),
            ("ors", HttpStatusCode.OK, orsResponse));
        var service = CreateService(handler, config);

        var result = await service.OptimizeAsync(
            [106.0, 10.0],
            [[106.1, 10.1], [106.2, 10.2]]);

        result.Should().Equal([1, 0]);
    }

    [Fact]
    public async Task OptimizeAsync_ApiError_ReturnsFallbackOrder()
    {
        var (handler, config) = CreateMocks(
            ("nominatim", HttpStatusCode.OK, "[]"),
            ("ors", HttpStatusCode.InternalServerError, ""));
        var service = CreateService(handler, config);

        var result = await service.OptimizeAsync(
            [106.0, 10.0],
            [[106.1, 10.1], [106.2, 10.2]]);

        result.Should().Equal([0, 1]);
    }

    [Fact]
    public async Task OptimizeAsync_NoApiKey_ReturnsFallbackOrder()
    {
        var config = new ConfigurationBuilder().Build();
        var handlerMock = new Mock<HttpMessageHandler>();
        var service = CreateService(handlerMock, config);

        var result = await service.OptimizeAsync(
            [106.0, 10.0],
            [[106.1, 10.1], [106.2, 10.2]]);

        result.Should().Equal([0, 1]);
    }

    [Fact]
    public async Task OptimizeAsync_EmptyJobs_ReturnsFallbackOrder()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenRouteService:ApiKey"] = "test-key",
            })
            .Build();
        var handlerMock = new Mock<HttpMessageHandler>();
        var service = CreateService(handlerMock, config);

        var result = await service.OptimizeAsync(
            [106.0, 10.0],
            []);

        result.Should().BeEmpty();
    }

    private static (Mock<HttpMessageHandler> Handler, IConfiguration Config) CreateMocks(
        (string Name, HttpStatusCode Status, string Body) client1,
        (string Name, HttpStatusCode Status, string Body) client2)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns((HttpRequestMessage request, CancellationToken _) =>
            {
                var url = request.RequestUri?.ToString() ?? "";
                if (url.Contains("nominatim"))
                    return Task.FromResult(new HttpResponseMessage
                    {
                        StatusCode = client1.Status,
                        Content = new StringContent(client1.Body),
                    });
                return Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = client2.Status,
                    Content = new StringContent(client2.Body),
                });
            });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenRouteService:ApiKey"] = "test-api-key",
            })
            .Build();

        return (handlerMock, config);
    }

    private static OpenRouteOptimizationService CreateService(
        Mock<HttpMessageHandler> handlerMock, IConfiguration config)
    {
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(f => f.CreateClient("nominatim"))
            .Returns(new HttpClient(handlerMock.Object));
        httpClientFactoryMock
            .Setup(f => f.CreateClient("ors"))
            .Returns(new HttpClient(handlerMock.Object));

        return new OpenRouteOptimizationService(httpClientFactoryMock.Object, config);
    }
}
