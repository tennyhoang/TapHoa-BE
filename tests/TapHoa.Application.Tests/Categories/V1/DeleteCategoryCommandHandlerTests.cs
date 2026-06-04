using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using TapHoa.Application.Categories.V1.DeleteCategory;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Categories.V1;

public class DeleteCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepoMock = new();
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly DeleteCategoryCommandHandler _handler;

    public DeleteCategoryCommandHandlerTests()
    {
        _handler = new DeleteCategoryCommandHandler(_categoryRepoMock.Object, _productRepoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_EmptyCategory_DeletesSuccessfully()
    {
        var id = Guid.NewGuid();
        var category = new Category { Name = "Test" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);
        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(false);

        await _handler.Handle(new DeleteCategoryCommand(id), CancellationToken.None);

        _categoryRepoMock.Verify(r => r.Remove(category), Times.Once);
        _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsKeyNotFoundException()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(new DeleteCategoryCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy danh mục.");
    }

    [Fact]
    public async Task Handle_CategoryWithProducts_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var category = new Category { Name = "Test" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);
        _productRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(new DeleteCategoryCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Không thể xóa danh mục đang có sản phẩm.");
    }

    [Fact]
    public async Task Handle_CategoryWithChildren_ThrowsInvalidOperationException()
    {
        var id = Guid.NewGuid();
        var category = new Category { Name = "Test" };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);
        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        var act = () => _handler.Handle(new DeleteCategoryCommand(id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Không thể xóa danh mục đang có danh mục con.");
    }
}
