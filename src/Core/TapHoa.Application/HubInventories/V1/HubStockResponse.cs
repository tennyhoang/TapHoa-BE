namespace TapHoa.Application.HubInventories.V1;

public record HubStockResponse(
    Guid HubId,
    string HubName,
    Guid ProductId,
    string ProductName,
    int Stock
);
