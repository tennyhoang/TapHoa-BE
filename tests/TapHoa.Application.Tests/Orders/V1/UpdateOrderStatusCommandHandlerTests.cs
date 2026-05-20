using FluentAssertions;
using MediatR;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Orders.V1.UpdateOrderStatus;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Orders.V1;

public class UpdateOrderStatusCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IHubInventoryRepository> _inventoryRepoMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly UpdateOrderStatusCommandHandler _handler;

    public UpdateOrderStatusCommandHandlerTests()
    {
        _handler = new UpdateOrderStatusCommandHandler(
            _orderRepoMock.Object, _inventoryRepoMock.Object, _publisherMock.Object);
    }

    private static Order BuildOrder(OrderStatus status, Guid? id = null)
    {
        var order = new Order
        {
            Hub = new Hub
            {
                Name = "Hub Test", Address = "123 Main St", Ward = "P1",
                District = "Q1", City = "HCM", Latitude = 0, Longitude = 0
            },
            User = new User { FullName = "Test User", Email = "test@example.com" },
            Items = []
        };
        typeof(Order).GetProperty("Status")!.SetValue(order, status);
        if (id.HasValue) typeof(Order).GetProperty("Id")!.SetValue(order, id.Value);
        return order;
    }

    [Fact]
    public async Task Handle_PendingToConfirmed_UpdatesStatus()
    {
        var id = Guid.NewGuid();
        var order = BuildOrder(OrderStatus.Pending, id);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(id, OrderStatus.Confirmed), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_InvalidTransition_ReturnsFailResult()
    {
        var id = Guid.NewGuid();
        var order = BuildOrder(OrderStatus.Delivered, id);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(id, OrderStatus.Cancelled), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TRANSITION");
    }

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailResult()
    {
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order>().BuildMockDbSet().Object);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(Guid.NewGuid(), OrderStatus.Confirmed), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
    }

    [Theory]
    [InlineData(OrderStatus.Pending,      OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Pending,      OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed,    OrderStatus.Shipping)]
    [InlineData(OrderStatus.Shipping,     OrderStatus.ArrivedAtHub)]
    [InlineData(OrderStatus.ArrivedAtHub, OrderStatus.Delivered)]
    public async Task Handle_ValidTransitions_Succeed(OrderStatus from, OrderStatus to)
    {
        var id = Guid.NewGuid();
        var order = BuildOrder(from, id);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new UpdateOrderStatusCommand(id, to), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(to);
    }
}
