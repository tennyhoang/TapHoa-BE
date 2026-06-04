using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Categories.V1.GetCategories;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Categories.V1;

public class GetCategoriesQueryHandlerTests
{
    private readonly Mock<IRepository<Category>> _categoryRepoMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTests()
    {
        _handler = new GetCategoriesQueryHandler(_categoryRepoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsRootCategoriesWithChildren()
    {
        var child = new Category { Name = "iPhone", Children = [] };
        var root = new Category { Name = "Điện thoại", Children = [child] };
        _categoryRepoMock.Setup(r => r.Query()).Returns(new List<Category> { root }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Điện thoại");
        result[0].Children[0].Name.Should().Be("iPhone");
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyList()
    {
        _categoryRepoMock.Setup(r => r.Query())
            .Returns(new List<Category>().BuildMockDbSet().Object);

        var result = await _handler.Handle(new GetCategoriesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
