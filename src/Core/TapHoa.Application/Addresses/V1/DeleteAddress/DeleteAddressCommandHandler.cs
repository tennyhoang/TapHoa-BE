using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Addresses.V1.DeleteAddress;

public class DeleteAddressCommandHandler(IRepository<Address> addressRepo)
    : IRequestHandler<DeleteAddressCommand>
{
    public async Task Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
    {
        var address = await addressRepo.FindAsync(
            a => a.Id == request.AddressId && a.UserId == request.UserId)
            ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ.");

        addressRepo.Remove(address);
        await addressRepo.SaveChangesAsync();
    }
}
