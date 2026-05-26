namespace TapHoa.Application.Users.V1.Admin.AssignWarehouse;

public record AssignWarehouseResponse(
    Guid    UserId,
    string  FullName,
    string  Role,
    Guid?   WarehouseId,
    Guid?   ManagedWarehouseId
);
