using MediatR;

namespace TapHoa.Application.Addresses.V1.SetDefaultAddress;

public record SetDefaultAddressCommand(Guid AddressId, Guid UserId) : IRequest<AddressResponse>;
