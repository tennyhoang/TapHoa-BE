using System.ComponentModel.DataAnnotations;
using MediatR;

namespace TapHoa.Application.Addresses.V1.AddAddress;

public record AddAddressCommand(
    Guid UserId,
    [Required] string ReceiverName,
    [Required] string PhoneNumber,
    [Required] string Province,
    [Required] string District,
    [Required] string Ward,
    [Required] string StreetAddress,
    bool IsDefault = false
) : IRequest<AddressResponse>;
