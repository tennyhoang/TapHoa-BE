using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Addresses.V1.GetMyAddresses;

public class GetMyAddressesQueryHandler(IRepository<Address> addressRepo)
    : IRequestHandler<GetMyAddressesQuery, List<AddressResponse>>
{
    public async Task<List<AddressResponse>> Handle(GetMyAddressesQuery request, CancellationToken cancellationToken)
    {
        var addresses = await addressRepo.Query()
            .Where(a => a.UserId == request.UserId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync(cancellationToken);

        return addresses.Select(MapToResponse).ToList();
    }

    internal static AddressResponse MapToResponse(Address a) => new(
        a.Id, a.ReceiverName, a.PhoneNumber,
        a.Province, a.District, a.Ward, a.StreetAddress, a.IsDefault
    );
}
