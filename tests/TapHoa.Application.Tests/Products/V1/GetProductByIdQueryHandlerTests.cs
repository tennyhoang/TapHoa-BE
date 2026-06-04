using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Products.V1.GetProductById;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Products.V1;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        _handler = new GetProductByIdQueryHandler(_productRepoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ActiveProduct_ReturnsResponse()
    {
        var id = Guid.NewGuid();
        var product = new Product
        {
            Name = "iPhone 15", Price = 25_000_000, IsActive = true,
            Category = new Category { Name = "Điện thoại" }, Images = [], Reviews = []
        };
        product.GetType().GetProperty("Id")!.SetValue(product, id);

        _productRepoMock.Setup(r => r.Query())
            .Returns(new List<Product> { product }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        result.Name.Should().Be("iPhone 15");
        result.Price.Should().Be(25_000_000);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        _productRepoMock.Setup(r => r.Query())
            .Returns(new List<Product>().BuildMockDbSet().Object);

        var act = () => _handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy sản phẩm.");
    }

    [Fact]
    public async Task Handle_InactiveProduct_ThrowsKeyNotFoundException()
    {
        var id = Guid.NewGuid();
        var product = new Product { IsActive = false, Category = new Category { Name = "Test" }, Images = [], Reviews = [] };
        product.GetType().GetProperty("Id")!.SetValue(product, id);

        _productRepoMock.Setup(r => r.Query())
            .Returns(new List<Product> { product }.BuildMockDbSet().Object);

        var act = () => _handler.Handle(new GetProductByIdQuery(id), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
