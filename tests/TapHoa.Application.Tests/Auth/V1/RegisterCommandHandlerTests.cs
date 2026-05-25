using FluentAssertions;
using Moq;
using TapHoa.Application.Auth.V1.Register;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Auth.V1;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly Mock<IJwtService> _jwtServiceMock = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepoMock.Object, _jwtServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsTokenAndUserInfo()
    {
        // Arrange
        var command = new RegisterCommand("Nguyen Van A", "test@example.com", "password123", null);

        _userRepoMock.Setup(r => r.AnyAsync(u => u.Email == command.Email))
                     .ReturnsAsync(false);
        _jwtServiceMock.Setup(j => j.GenerateToken(It.IsAny<User>()))
                       .Returns("fake-jwt-token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("fake-jwt-token");
        result.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Nguyen Van A");

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new RegisterCommand("Nguyen Van A", "duplicate@example.com", "password123", null);

        _userRepoMock.Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                     .ReturnsAsync(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("Email đã được sử dụng.");

        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }
}
