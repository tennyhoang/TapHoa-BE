using FluentAssertions;
using Moq;
using TapHoa.Application.Users.V1.UpdateMyProfile;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Users.V1;

public class UpdateMyProfileCommandHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly UpdateMyProfileCommandHandler _handler;

    public UpdateMyProfileCommandHandlerTests()
    {
        _handler = new UpdateMyProfileCommandHandler(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidUser_UpdatesProfile()
    {
        var userId = Guid.NewGuid();
        var user = new User { FullName = "Old Name", Email = "test@test.com" };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _handler.Handle(
            new UpdateMyProfileCommand(userId, "New Name", "0909123456", null),
            CancellationToken.None);

        result.FullName.Should().Be("New Name");
        result.PhoneNumber.Should().Be("0909123456");
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var act = () => _handler.Handle(
            new UpdateMyProfileCommand(Guid.NewGuid(), "Name", null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy người dùng.");
    }
}
