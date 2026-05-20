using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Addresses.V1.SetDefaultAddress;

public class SetDefaultAddressCommandHandler(IRepository<Address> addressRepo)
    : IRequestHandler<SetDefaultAddressCommand, AddressResponse>
{
    public async Task<AddressResponse> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        var target = await addressRepo.FindAsync(
            a => a.Id == request.AddressId && a.UserId == request.UserId)
            ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ.");

        var allAddresses = await addressRepo.Query()
            .Where(a => a.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var addr in allAddresses)
        {
            addr.IsDefault = addr.Id == request.AddressId;
            addressRepo.Update(addr);
        }

        await addressRepo.SaveChangesAsync();

        target.IsDefault = true;
        return GetMyAddresses.GetMyAddressesQueryHandler.MapToResponse(target);
    }
}
