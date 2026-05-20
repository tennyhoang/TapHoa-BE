using FluentAssertions;
using Moq;
using TapHoa.Application.Addresses.V1.DeleteAddress;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Tests.Addresses.V1;

public class DeleteAddressCommandHandlerTests
{
    private readonly Mock<IRepository<Address>> _addressRepoMock = new();
    private readonly DeleteAddressCommandHandler _handler;

    public DeleteAddressCommandHandlerTests()
    {
        _handler = new DeleteAddressCommandHandler(_addressRepoMock.Object);
    }

    [Fact]
    public async Task Handle_OwnAddress_DeletesSuccessfully()
    {
        var addressId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var address = new Address { UserId = userId, ReceiverName = "Test" };

        _addressRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Address, bool>>>()))
            .ReturnsAsync(address);

        await _handler.Handle(new DeleteAddressCommand(addressId, userId), CancellationToken.None);

        _addressRepoMock.Verify(r => r.Remove(address), Times.Once);
        _addressRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_AddressNotFound_ThrowsKeyNotFoundException()
    {
        _addressRepoMock.Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Address, bool>>>()))
            .ReturnsAsync((Address?)null);

        var act = () => _handler.Handle(new DeleteAddressCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Không tìm thấy địa chỉ.");
    }
}
