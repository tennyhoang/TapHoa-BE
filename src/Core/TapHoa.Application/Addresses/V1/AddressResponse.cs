namespace TapHoa.Application.Addresses.V1;

public record AddressResponse(
    Guid Id,
    string ReceiverName,
    string PhoneNumber,
    string Province,
    string District,
    string Ward,
    string StreetAddress,
    bool IsDefault
);
