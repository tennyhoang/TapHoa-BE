using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Addresses.V1.AddAddress;

public class AddAddressCommandHandler(IRepository<Address> addressRepo)
    : IRequestHandler<AddAddressCommand, AddressResponse>
{
    public async Task<AddressResponse> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        // Nếu set default, bỏ default của địa chỉ cũ
        if (request.IsDefault)
        {
            var existing = await addressRepo.Query()
                .Where(a => a.UserId == request.UserId && a.IsDefault)
                .ToListAsync(cancellationToken);

            foreach (var addr in existing)
            {
                addr.IsDefault = false;
                addressRepo.Update(addr);
            }
        }

        var address = new Address
        {
            UserId = request.UserId,
            ReceiverName = request.ReceiverName,
            PhoneNumber = request.PhoneNumber,
            Province = request.Province,
            District = request.District,
            Ward = request.Ward,
            StreetAddress = request.StreetAddress,
            IsDefault = request.IsDefault
        };

        await addressRepo.AddAsync(address);
        await addressRepo.SaveChangesAsync();

        return GetMyAddresses.GetMyAddressesQueryHandler.MapToResponse(address);
    }
}
