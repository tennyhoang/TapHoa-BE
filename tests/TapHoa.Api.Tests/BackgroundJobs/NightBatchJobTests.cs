using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using TapHoa.Api.BackgroundJobs;

namespace TapHoa.Api.Tests.BackgroundJobs;

public class NightBatchJobTests
{
    [Fact]
    public async Task StartStop_StopsCleanly()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<NightBatchJob>>();
        var job = new NightBatchJob(scopeFactoryMock.Object, loggerMock.Object);

        await job.StartAsync(CancellationToken.None);

        await Task.Delay(50);

        await job.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var loggerMock = new Mock<ILogger<NightBatchJob>>();

        var act = () => new NightBatchJob(scopeFactoryMock.Object, loggerMock.Object);

        act.Should().NotThrow();
    }
}
