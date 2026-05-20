using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Cart.V1.ClearCart;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Cart.V1;

public class ClearCartCommandHandlerTests
{
    private readonly Mock<IRepository<CartItem>> _cartRepoMock = new();
    private readonly ClearCartCommandHandler _handler;

    public ClearCartCommandHandlerTests()
    {
        _handler = new ClearCartCommandHandler(_cartRepoMock.Object);
    }

    [Fact]
    public async Task Handle_CartWithItems_RemovesAll()
    {
        var userId = Guid.NewGuid();
        var items = new List<CartItem>
        {
            new() { UserId = userId, ProductId = Guid.NewGuid(), Quantity = 1 },
            new() { UserId = userId, ProductId = Guid.NewGuid(), Quantity = 2 }
        };
        _cartRepoMock.Setup(r => r.Query()).Returns(items.BuildMockDbSet().Object);

        await _handler.Handle(new ClearCartCommand(userId), CancellationToken.None);

        _cartRepoMock.Verify(r => r.Remove(It.IsAny<CartItem>()), Times.Exactly(2));
        _cartRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_EmptyCart_SavesWithoutRemoving()
    {
        var userId = Guid.NewGuid();
        _cartRepoMock.Setup(r => r.Query()).Returns(new List<CartItem>().BuildMockDbSet().Object);

        await _handler.Handle(new ClearCartCommand(userId), CancellationToken.None);

        _cartRepoMock.Verify(r => r.Remove(It.IsAny<CartItem>()), Times.Never);
        _cartRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
