using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Products.V1.CreateProduct;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Products.V1;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _productRepoMock = new();
    private readonly Mock<IRepository<Category>> _categoryRepoMock = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productRepoMock.Object, _categoryRepoMock.Object);
    }

    [Fact]
    public async Task Handle_InvalidCategory_ThrowsKeyNotFoundException()
    {
        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(false);

        // Name, Description, Price, DiscountPrice, Stock, ThumbnailUrl, Images, CategoryId
        var command = new CreateProductCommand("Phone", null, 1000, null, 10, null, [], Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Danh mục không tồn tại.");
    }

    [Fact]
    public async Task Handle_ValidInput_CreatesProductAndReturnsResponse()
    {
        var categoryId = Guid.NewGuid();
        var category = new Category { Name = "Điện thoại", Children = [] };
        category.GetType().GetProperty("Id")!.SetValue(category, categoryId);

        _categoryRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Category, bool>>>()))
            .ReturnsAsync(true);

        var products = new List<Product>();
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => { p.Category = category; p.Images = []; p.Reviews = []; products.Add(p); })
            .Returns(Task.CompletedTask);
        _productRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _productRepoMock.Setup(r => r.Query()).Returns(() => products.BuildMockDbSet().Object);

        var command = new CreateProductCommand("iPhone 15", "Mô tả", 25_000_000, null, 50, null, [], categoryId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("iPhone 15");
        result.CategoryId.Should().Be(categoryId);
        result.Price.Should().Be(25_000_000);
    }
}
