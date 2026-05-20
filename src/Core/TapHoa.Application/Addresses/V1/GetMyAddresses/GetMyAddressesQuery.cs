using MediatR;

namespace TapHoa.Application.Addresses.V1.GetMyAddresses;

public record GetMyAddressesQuery(Guid UserId) : IRequest<List<AddressResponse>>;
