using MediatR;

namespace TapHoa.Application.Addresses.V1.DeleteAddress;

public record DeleteAddressCommand(Guid AddressId, Guid UserId) : IRequest;
