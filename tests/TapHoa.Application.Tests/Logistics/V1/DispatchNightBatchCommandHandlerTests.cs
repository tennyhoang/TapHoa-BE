using FluentAssertions;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Logistics.V1.DispatchNightBatch;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Logistics.V1;

public class DispatchNightBatchCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock = new();
    private readonly Mock<ILogger<DispatchNightBatchCommandHandler>> _loggerMock = new();
    private readonly DispatchNightBatchCommandHandler _handler;

    public DispatchNightBatchCommandHandlerTests()
    {
        _handler = new DispatchNightBatchCommandHandler(
            _orderRepoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_EligibleOrdersFound_DispatchesAll()
    {
        var hubId = Guid.NewGuid();
        var orders = new List<Order>
        {
            BuildOrder(OrderStatus.Paid_WaitingForBatch, hubId),
            BuildOrder(OrderStatus.Paid_WaitingForBatch, hubId),
        };
        _orderRepoMock.Setup(r => r.Query()).Returns(orders.BuildMockDbSet().Object);

        var result = await _handler.Handle(new DispatchNightBatchCommand(), CancellationToken.None);

        result.TotalOrders.Should().Be(2);
        result.TotalHubs.Should().Be(1);
        orders.Should().OnlyContain(o => o.Status == OrderStatus.ShippingToHub);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NoEligibleOrders_ReturnsZero()
    {
        var orders = new List<Order>();
        _orderRepoMock.Setup(r => r.Query()).Returns(orders.BuildMockDbSet().Object);

        var result = await _handler.Handle(new DispatchNightBatchCommand(), CancellationToken.None);

        result.TotalOrders.Should().Be(0);
        result.TotalHubs.Should().Be(0);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongStatus_NotDispatched()
    {
        var orders = new List<Order>
        {
            BuildOrder(OrderStatus.PendingPayment, Guid.NewGuid()),
            BuildOrder(OrderStatus.InHub_ReadyForPickup, Guid.NewGuid()),
        };
        _orderRepoMock.Setup(r => r.Query()).Returns(orders.BuildMockDbSet().Object);

        var result = await _handler.Handle(new DispatchNightBatchCommand(), CancellationToken.None);

        result.TotalOrders.Should().Be(0);
    }

    [Fact]
    public async Task Handle_MultipleHubs_CountsCorrectly()
    {
        var orders = new List<Order>
        {
            BuildOrder(OrderStatus.Paid_WaitingForBatch, Guid.NewGuid()),
            BuildOrder(OrderStatus.Paid_WaitingForBatch, Guid.NewGuid()),
        };
        _orderRepoMock.Setup(r => r.Query()).Returns(orders.BuildMockDbSet().Object);

        var result = await _handler.Handle(new DispatchNightBatchCommand(), CancellationToken.None);

        result.TotalOrders.Should().Be(2);
        result.TotalHubs.Should().Be(2);
    }

    private static Order BuildOrder(OrderStatus status, Guid hubId)
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            TotalAmount = 100000,
            PaymentRef = "TH12345678",
            HubId = hubId,
            Hub = new Hub
            {
                Name = "Test Hub",
                Address = "123 Street",
                Ward = "Ward 1",
                District = "District 1",
                City = "HCM",
                Latitude = 10.0,
                Longitude = 106.0,
            },
            Items = [],
        };
        typeof(Order).GetProperty("Status")!.SetValue(order, status);
        return order;
    }
}
