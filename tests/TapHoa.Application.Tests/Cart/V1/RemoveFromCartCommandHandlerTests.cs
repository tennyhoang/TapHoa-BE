using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Cart.V1.RemoveFromCart;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Cart.V1;

public class RemoveFromCartCommandHandlerTests
{
    private readonly Mock<IRepository<CartItem>> _cartRepoMock = new();
    private readonly RemoveFromCartCommandHandler _handler;

    public RemoveFromCartCommandHandlerTests()
    {
        _handler = new RemoveFromCartCommandHandler(_cartRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ItemExists_RemovesFromCart()
    {
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var item = new CartItem { UserId = userId, ProductId = productId, Quantity = 1 };

        _cartRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync(item);
        _cartRepoMock.Setup(r => r.Query()).Returns(new List<CartItem>().BuildMockDbSet().Object);

        await _handler.Handle(new RemoveFromCartCommand(userId, productId), CancellationToken.None);

        _cartRepoMock.Verify(r => r.Remove(item), Times.Once);
        _cartRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsKeyNotFoundException()
    {
        _cartRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<CartItem, bool>>>()))
            .ReturnsAsync((CartItem?)null);

        var act = () => _handler.Handle(new RemoveFromCartCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Sản phẩm không có trong giỏ hàng.");
    }
}
