using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using TapHoa.Worker.Consumers;
using TapHoa.Worker.Messages;

namespace TapHoa.Worker.Tests.Consumers;

public class OrderPaidConsumerTests
{
    [Fact]
    public async Task Consume_LogsOrderInformation()
    {
        var loggerMock = new Mock<ILogger<OrderPaidConsumer>>();
        var consumer = new OrderPaidConsumer(loggerMock.Object);
        var message = new OrderPaidMessage(
            OrderId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Amount: 100_000m,
            PaidAt: DateTime.UtcNow);
        var contextMock = new Mock<ConsumeContext<OrderPaidMessage>>();
        contextMock.Setup(c => c.Message).Returns(message);

        await consumer.Consume(contextMock.Object);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message.OrderId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var loggerMock = new Mock<ILogger<OrderPaidConsumer>>();
        var act = () => new OrderPaidConsumer(loggerMock.Object);
        act.Should().NotThrow();
    }
}
