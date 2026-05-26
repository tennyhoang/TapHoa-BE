namespace TapHoa.Application.Driver.V1.GetDriverWarehouse;

public record AssignedWarehouseDto(
    Guid    Id,
    string  Name,
    string  FullAddress,
    string? PhoneNumber
);
