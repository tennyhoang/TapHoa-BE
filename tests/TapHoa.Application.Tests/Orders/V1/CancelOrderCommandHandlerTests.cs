using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Orders.V1.CancelOrder;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Orders.V1;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IHubInventoryRepository> _inventoryRepoMock = new();
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _inventoryRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync((HubInventory?)null);

        _handler = new CancelOrderCommandHandler(
            _orderRepoMock.Object, _productRepoMock.Object, _inventoryRepoMock.Object);
    }

    private static Order BuildOrder(OrderStatus status, Guid userId, Guid? id = null)
    {
        var order = new Order
        {
            UserId = userId,
            Hub = new Hub
            {
                Name = "Hub Test", Address = "1 Main", Ward = "P1",
                District = "Q1", City = "HCM", Latitude = 0, Longitude = 0
            },
            Items = []
        };
        typeof(Order).GetProperty("Status")!.SetValue(order, status);
        if (id.HasValue) typeof(Order).GetProperty("Id")!.SetValue(order, id.Value);
        return order;
    }

    [Fact]
    public async Task Handle_PendingOrder_CancelsAndRestoresStock()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var product = new Product { Name = "Phone", Stock = 5 };
        var order = BuildOrder(OrderStatus.PendingPayment, userId, orderId);
        order.Items = [new OrderItem { Product = product, Quantity = 2, UnitPrice = 1000 }];

        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new CancelOrderCommand(orderId, userId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be(OrderStatus.Cancelled);
        product.Stock.Should().Be(7);
        _productRepoMock.Verify(r => r.Update(product), Times.Once);
    }

    [Fact]
    public async Task Handle_ConfirmedOrder_ReturnsFailResult()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = BuildOrder(OrderStatus.ShippingToHub, userId, orderId);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new CancelOrderCommand(orderId, userId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TRANSITION");
    }

    [Fact]
    public async Task Handle_OrderNotFound_ReturnsFailResult()
    {
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order>().BuildMockDbSet().Object);

        var result = await _handler.Handle(new CancelOrderCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("ORDER_NOT_FOUND");
    }
}
