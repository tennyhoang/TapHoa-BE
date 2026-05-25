using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TapHoa.Application.Addresses.V1.SetDefaultAddress;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Addresses.V1;

public class SetDefaultAddressCommandHandlerTests
{
    private readonly Mock<IRepository<Address>> _addressRepoMock = new();
    private readonly SetDefaultAddressCommandHandler _handler;

    public SetDefaultAddressCommandHandlerTests()
    {
        _handler = new SetDefaultAddressCommandHandler(_addressRepoMock.Object);
    }

    [Fact]
    public async Task Handle_ValidAddress_SetsAsDefaultAndClearsOthers()
    {
        var userId = Guid.NewGuid();
        var addressId = Guid.NewGuid();
        var target = new Address { UserId = userId, ReceiverName = "Target", PhoneNumber = "0900000000", Province = "HCM", District = "Q1", Ward = "P1", StreetAddress = "1", IsDefault = false };
        target.GetType().GetProperty("Id")!.SetValue(target, addressId);
        var other = new Address { UserId = userId, ReceiverName = "Other", PhoneNumber = "0900000001", Province = "HN", District = "HK", Ward = "P2", StreetAddress = "2", IsDefault = true };

        _addressRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Address, bool>>>()))
            .ReturnsAsync(target);
        _addressRepoMock.Setup(r => r.Query())
            .Returns(new List<Address> { target, other }.BuildMockDbSet().Object);

        var result = await _handler.Handle(new SetDefaultAddressCommand(addressId, userId), CancellationToken.None);

        result.IsDefault.Should().BeTrue();
        other.IsDefault.Should().BeFalse();
        _addressRepoMock.Verify(r => r.Update(It.IsAny<Address>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_AddressNotFound_ThrowsKeyNotFoundException()
    {
        _addressRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Address, bool>>>()))
            .ReturnsAsync((Address?)null);

        var act = () => _handler.Handle(new SetDefaultAddressCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy địa chỉ.");
    }
}
