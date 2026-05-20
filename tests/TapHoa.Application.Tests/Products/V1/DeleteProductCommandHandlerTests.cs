using FluentAssertions;
using Moq;
using TapHoa.Application.Products.V1.DeleteProduct;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Products.V1;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(_productRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingProduct_SetsIsActiveFalse()
    {
        var id = Guid.NewGuid();
        var product = new Product { Name = "Test", IsActive = true };
        _productRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(product);

        await _handler.Handle(new DeleteProductCommand(id), CancellationToken.None);

        product.IsActive.Should().BeFalse();
        _productRepoMock.Verify(r => r.Update(product), Times.Once);
        _productRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Product?)null);

        var act = () => _handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy sản phẩm.");
    }
}
