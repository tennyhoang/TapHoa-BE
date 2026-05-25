using FluentAssertions;
using Moq;
using TapHoa.Application.Users.V1.GetMyProfile;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Users.V1;

public class GetMyProfileQueryHandlerTests
{
    private readonly Mock<IRepository<User>> _userRepoMock = new();
    private readonly GetMyProfileQueryHandler _handler;

    public GetMyProfileQueryHandlerTests()
    {
        _handler = new GetMyProfileQueryHandler(_userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_ReturnsProfile()
    {
        var userId = Guid.NewGuid();
        var user = new User { FullName = "Nguyen Van A", Email = "a@test.com", PhoneNumber = "0901234567" };
        user.GetType().GetProperty("Id")!.SetValue(user, userId);

        _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var result = await _handler.Handle(new GetMyProfileQuery(userId), CancellationToken.None);

        result.FullName.Should().Be("Nguyen Van A");
        result.Email.Should().Be("a@test.com");
        result.PhoneNumber.Should().Be("0901234567");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);

        var act = () => _handler.Handle(new GetMyProfileQuery(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy người dùng.");
    }
}
