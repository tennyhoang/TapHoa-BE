using FluentAssertions;
using Moq;
using TapHoa.Application.Users.V1.ChangePassword;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Users.V1;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _handler = new ChangePasswordCommandHandler(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCurrentPassword_ChangesPassword()
    {
        var userId = Guid.NewGuid();
        var user = new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass") };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        await _handler.Handle(new ChangePasswordCommand(userId, "oldpass", "newpass123"), CancellationToken.None);

        BCrypt.Net.BCrypt.Verify("newpass123", user.PasswordHash).Should().BeTrue();
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongCurrentPassword_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var user = new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpass") };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var act = () => _handler.Handle(new ChangePasswordCommand(userId, "wrongpass", "newpass"), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Mật khẩu hiện tại không đúng.");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var act = () => _handler.Handle(new ChangePasswordCommand(Guid.NewGuid(), "pass", "newpass"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
