using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Orders.V1.ConfirmPayment;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Orders.V1;

public class ConfirmPaymentCommandHandlerTests
{
    private readonly Mock<IRepository<Order>> _orderRepoMock = new();
    private readonly Mock<IRepository<WalletTopupRequest>> _topupRepoMock = new();
    private readonly Mock<IRepository<WalletTransaction>> _walletTxRepoMock = new();
    private readonly ConfirmPaymentCommandHandler _handler;

    public ConfirmPaymentCommandHandlerTests()
    {
        _handler = new ConfirmPaymentCommandHandler(
            _orderRepoMock.Object, _topupRepoMock.Object, _walletTxRepoMock.Object);
    }

    [Fact]
    public async Task Handle_TransferTypeNotIn_ReturnsFalse()
    {
        var payload = ValidPayload() with { TransferType = "out" };

        var result = await _handler.Handle(new ConfirmPaymentCommand(payload), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_MatchingOrder_ConfirmsPayment()
    {
        var order = BuildOrder("TH12345678");
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order> { order }.BuildMockDbSet().Object);

        var payload = ValidPayload() with { Content = "TH12345678 chuyen tien" };

        var result = await _handler.Handle(new ConfirmPaymentCommand(payload), CancellationToken.None);

        result.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Paid_WaitingForBatch);
        order.PaidAt.Should().NotBeNull();
        _orderRepoMock.Verify(r => r.Update(order), Times.Once);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NoMatchingOrder_SkipsOrderPayment()
    {
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order>().BuildMockDbSet().Object);

        var payload = ValidPayload() with { Content = "NONEXISTENTREF" };

        var result = await _handler.Handle(new ConfirmPaymentCommand(payload), CancellationToken.None);

        result.Should().BeFalse();
        _orderRepoMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_MatchingTopup_CreditsWallet()
    {
        var user = new User();
        var topup = new WalletTopupRequest
        {
            UserId = Guid.NewGuid(),
            Amount = 50000,
            PaymentRef = "THWLABCD123",
            Status = WalletTopupStatus.Pending,
            User = user,
        };
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order>().BuildMockDbSet().Object);
        _topupRepoMock.Setup(r => r.Query()).Returns(new List<WalletTopupRequest> { topup }.BuildMockDbSet().Object);

        var payload = ValidPayload() with { Content = "THWLABCD123 nap tien", TransferAmount = 50000 };

        var result = await _handler.Handle(new ConfirmPaymentCommand(payload), CancellationToken.None);

        result.Should().BeTrue();
        user.WalletBalance.Should().Be(50000);
        topup.Status.Should().Be(WalletTopupStatus.Completed);
        _topupRepoMock.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
        _walletTxRepoMock.Verify(r => r.AddAsync(It.Is<WalletTransaction>(t =>
            t.Amount == 50000 && t.Type == WalletTransactionType.Credit)), Times.Once);
    }

    [Fact]
    public async Task Handle_ContentDoesNotContainTHWL_ReturnsFalse()
    {
        _orderRepoMock.Setup(r => r.Query()).Returns(new List<Order>().BuildMockDbSet().Object);
        _topupRepoMock.Setup(r => r.Query()).Returns(new List<WalletTopupRequest>().BuildMockDbSet().Object);

        var payload = ValidPayload() with { Content = "random payment without prefix" };

        var result = await _handler.Handle(new ConfirmPaymentCommand(payload), CancellationToken.None);

        result.Should().BeFalse();
    }

    private static SepayWebhookPayload ValidPayload() => new(
        Id: 1,
        Gateway: "VCB",
        TransactionDate: "2026-06-03 12:00:00",
        AccountNumber: "123456789",
        Content: "payment",
        TransferType: "in",
        TransferAmount: 100000,
        ReferenceCode: "REF001");

    private static Order BuildOrder(string paymentRef)
    {
        var order = new Order
        {
            UserId = Guid.NewGuid(),
            TotalAmount = 100000,
            PaymentRef = paymentRef,
            HubId = Guid.NewGuid(),
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
        typeof(Order).GetProperty("Status")!.SetValue(order, OrderStatus.PendingPayment);
        return order;
    }
}
