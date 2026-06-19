using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using TapHoa.Api.Filters;
using TapHoa.Infrastructure.Payment;

namespace TapHoa.Api.Tests.Filters;

public class SepayWebhookSecretFilterTests
{
    [Fact]
    public async Task InvokeAsync_SecretKeyNotConfigured_Returns503()
    {
        var filter = CreateFilter(new SepayOptions { ApiKey = "key", SecretKey = "" });
        var invoked = false;
        var next = CreateNext(() => invoked = true);

        var result = await filter.InvokeAsync(CreateContext(new HeaderDictionary()), next);

        invoked.Should().BeFalse();
        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.StatusCodeHttpResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task InvokeAsync_CorrectSecret_PassesThrough()
    {
        var filter = CreateFilter(new SepayOptions { ApiKey = "key", SecretKey = "my-secret" });
        var invoked = false;
        var next = CreateNext(() => invoked = true);
        var headers = new HeaderDictionary
        {
            { "x-sepay-secret-key", "my-secret" },
        };

        await filter.InvokeAsync(CreateContext(headers), next);

        invoked.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WrongSecret_ReturnsUnauthorized()
    {
        var filter = CreateFilter(new SepayOptions { ApiKey = "key", SecretKey = "my-secret" });
        var next = CreateNext(() => throw new InvalidOperationException("Should not reach next"));
        var headers = new HeaderDictionary
        {
            { "x-sepay-secret-key", "wrong-secret" },
        };

        var result = await filter.InvokeAsync(CreateContext(headers), next);

        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>();
    }

    [Fact]
    public async Task InvokeAsync_MissingSecretHeader_ReturnsUnauthorized()
    {
        var filter = CreateFilter(new SepayOptions { ApiKey = "key", SecretKey = "my-secret" });
        var next = CreateNext(() => throw new InvalidOperationException("Should not reach next"));

        var result = await filter.InvokeAsync(CreateContext(new HeaderDictionary()), next);

        result.Should().BeOfType<Microsoft.AspNetCore.Http.HttpResults.UnauthorizedHttpResult>();
    }

    private static SepayWebhookSecretFilter CreateFilter(SepayOptions options)
    {
        var optionsMock = new Mock<IOptions<SepayOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);
        return new SepayWebhookSecretFilter(optionsMock.Object);
    }

    private static EndpointFilterInvocationContext CreateContext(IHeaderDictionary headers)
    {
        var httpContext = new DefaultHttpContext();
        foreach (var (key, value) in headers)
            httpContext.Request.Headers[key] = value;

        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.Setup(c => c.HttpContext).Returns(httpContext);
        return contextMock.Object;
    }

    private static EndpointFilterDelegate CreateNext(Action onInvoked)
    {
        return (EndpointFilterInvocationContext _) =>
        {
            onInvoked();
            return ValueTask.FromResult<object?>("ok");
        };
    }
}
