using FluentAssertions;
using Moq;
using TapHoa.Application.Categories.V1.CreateCategory;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Categories.V1;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepoMock = new();
    private readonly CreateCategoryCommandHandler _handler;

    public CreateCategoryCommandHandlerTests()
    {
        _handler = new CreateCategoryCommandHandler(_categoryRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WithoutParent_CreatesCategory()
    {
        _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
        _categoryRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new CreateCategoryCommand("Điện thoại", "Danh mục điện thoại", null, null);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Điện thoại");
        result.Description.Should().Be("Danh mục điện thoại");
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidParent_ThrowsKeyNotFoundException()
    {
        var parentId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);

        var command = new CreateCategoryCommand("Sub", null, null, parentId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Danh mục cha không tồn tại.");
    }

    [Fact]
    public async Task Handle_WithValidParent_CreatesSubCategory()
    {
        var parentId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);
        _categoryRepoMock.Setup(r => r.AddAsync(It.IsAny<Category>())).Returns(Task.CompletedTask);
        _categoryRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var command = new CreateCategoryCommand("iPhone", null, null, parentId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("iPhone");
        result.ParentId.Should().Be(parentId);
    }
}
