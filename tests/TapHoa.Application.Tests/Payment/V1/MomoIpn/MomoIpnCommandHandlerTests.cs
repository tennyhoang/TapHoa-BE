using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Payment;
using TapHoa.Application.Payment.V1.MomoIpn;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Payment.V1.MomoIpn;

public class MomoIpnCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IMomoService> _momoServiceMock = new();
    private readonly MomoIpnCommandHandler _handler;

    public MomoIpnCommandHandlerTests()
    {
        _handler = new MomoIpnCommandHandler(_orderRepoMock.Object, _momoServiceMock.Object);
    }

    [Fact]
    public async Task Handle_MissingParameters_ReturnsFalse()
    {
        var parameters = new Dictionary<string, string>
        {
            { "orderId", "REF001_1749619200" },
        };

        var result = await _handler.Handle(new MomoIpnCommand(parameters), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidSignature_ReturnsFalse()
    {
        var parameters = new Dictionary<string, string>
        {
            { "signature", "badsig" },
            { "orderId", "REF001_1749619200" },
            { "errorCode", "0" },
        };
        _momoServiceMock.Setup(s => s.VerifyIpn(parameters, "badsig")).Returns(false);

        var result = await _handler.Handle(new MomoIpnCommand(parameters), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoMatchingOrder_ReturnsFalse()
    {
        var parameters = new Dictionary<string, string>
        {
            { "signature", "goodsig" },
            { "orderId", "REF001_1749619200" },
            { "errorCode", "0" },
        };
        _momoServiceMock.Setup(s => s.VerifyIpn(parameters, "goodsig")).Returns(true);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order>().BuildMockDbSet().Object);

        var result = await _handler.Handle(new MomoIpnCommand(parameters), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ValidResponseCode_ConfirmsOrder()
    {
        var order = BuildOrder("REF001", OrderStatus.AwaitingPayment);
        var parameters = new Dictionary<string, string>
        {
            { "signature", "goodsig" },
            { "orderId", "REF001_1749619200" },
            { "errorCode", "0" },
        };
        _momoServiceMock.Setup(s => s.VerifyIpn(parameters, "goodsig")).Returns(true);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new MomoIpnCommand(parameters), CancellationToken.None);

        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid_WaitingForBatch);
        order.PaidAt.Should().NotBeNull();
        _orderRepoMock.Verify(r => r.Update(order), Times.Once);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_FailedErrorCode_DoesNotConfirmOrder()
    {
        var order = BuildOrder("REF002", OrderStatus.AwaitingPayment);
        var parameters = new Dictionary<string, string>
        {
            { "signature", "goodsig" },
            { "orderId", "REF002_1749619200" },
            { "errorCode", "99" },
        };
        _momoServiceMock.Setup(s => s.VerifyIpn(parameters, "goodsig")).Returns(true);
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new MomoIpnCommand(parameters), CancellationToken.None);

        result.Should().BeFalse();
        order.Status.Should().Be(OrderStatus.AwaitingPayment);
        _orderRepoMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    private static Order BuildOrder(string paymentRef, OrderStatus status)
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            TotalAmount = 100000,
            PaymentRef = paymentRef,
            HubId = Guid.NewGuid(),
            Hub = new Hub
            {
                Name = "Test Hub", Address = "123 Street",
                Ward = "Ward 1", District = "District 1",
                City = "HCM", Latitude = 10.0, Longitude = 106.0,
            },
            Items = [],
        };
        typeof(Order).GetProperty("Status")!.SetValue(order, status);
        return order;
    }
}
