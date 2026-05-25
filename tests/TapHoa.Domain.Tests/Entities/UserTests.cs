using FluentAssertions;
using TapHoa.Domain.Entities;

namespace TapHoa.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void NewUser_ShouldHaveDefaultValues()
    {
        // Act
        var user = new User
        {
            FullName = "Nguyen Van A",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        // Assert
        user.Id.Should().NotBeEmpty();
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void NewUser_ShouldHaveEmptyCollections()
    {
        var user = new User { FullName = "A", Email = "a@b.com", PasswordHash = "h" };

        user.Addresses.Should().BeEmpty();
        user.Orders.Should().BeEmpty();
        user.CartItems.Should().BeEmpty();
        user.Reviews.Should().BeEmpty();
    }
}
