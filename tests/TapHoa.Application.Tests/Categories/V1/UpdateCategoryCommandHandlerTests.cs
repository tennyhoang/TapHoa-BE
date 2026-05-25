using FluentAssertions;
using Moq;
using TapHoa.Application.Categories.V1.UpdateCategory;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Categories.V1;

public class UpdateCategoryCommandHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepoMock = new();
    private readonly UpdateCategoryCommandHandler _handler;

    public UpdateCategoryCommandHandlerTests()
    {
        _handler = new UpdateCategoryCommandHandler(_categoryRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingCategory_UpdatesFields()
    {
        var id = Guid.NewGuid();
        var category = new Category { Name = "Old", Children = [] };
        _categoryRepoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(category);

        var result = await _handler.Handle(
            new UpdateCategoryCommand(id, "New Name", "Mô tả mới", null),
            CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("Mô tả mới");
        _categoryRepoMock.Verify(r => r.Update(category), Times.Once);
        _categoryRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsKeyNotFoundException()
    {
        _categoryRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Category?)null);

        var act = () => _handler.Handle(
            new UpdateCategoryCommand(Guid.NewGuid(), "Name", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy danh mục.");
    }
}
