using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TapHoa.Application.Loyalty;
using TapHoa.Application.Orders.V1.Events;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Orders.V1.Events;

public class EarnLoyaltyPointsHandlerTests
{
    private readonly Mock<ILoyaltyRepository> _loyaltyRepoMock = new();
    private readonly Mock<ILogger<EarnLoyaltyPointsHandler>> _loggerMock = new();
    private readonly LoyaltyOptions _options;
    private readonly EarnLoyaltyPointsHandler _handler;

    public EarnLoyaltyPointsHandlerTests()
    {
        _options = new LoyaltyOptions { EarnPerUnit = 10_000, RedeemValuePerPoint = 200 };
        _handler = new EarnLoyaltyPointsHandler(
            _loyaltyRepoMock.Object,
            _loggerMock.Object,
            Options.Create(_options));
    }

    [Fact]
    public async Task Handle_AboveThreshold_EarnsPoints()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var evt = new OrderCompletedEvent(orderId, userId, "a@a.com", "A", 150_000, DateTime.UtcNow);

        await _handler.Handle(evt, CancellationToken.None);

        _loyaltyRepoMock.Verify(r => r.EarnAsync(
            userId, 15, orderId,
            It.Is<string>(s => s.StartsWith("Tích điểm đơn hàng #")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BelowThreshold_DoesNotEarn()
    {
        var evt = new OrderCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), "b@b.com", "B", 5_000, DateTime.UtcNow);

        await _handler.Handle(evt, CancellationToken.None);

        _loyaltyRepoMock.Verify(r => r.EarnAsync(
            It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ExactlyAtThreshold_EarnsOnePoint()
    {
        var evt = new OrderCompletedEvent(Guid.NewGuid(), Guid.NewGuid(), "c@c.com", "C", 10_000, DateTime.UtcNow);

        await _handler.Handle(evt, CancellationToken.None);

        _loyaltyRepoMock.Verify(r => r.EarnAsync(
            It.IsAny<Guid>(), 1, It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
