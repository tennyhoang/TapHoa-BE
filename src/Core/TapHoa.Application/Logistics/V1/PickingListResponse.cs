namespace TapHoa.Application.Logistics.V1;

public record PickingLineItem(
    Guid ProductId,
    string ProductName,
    int TotalQuantity,
    int OrderCount
);

public record PickingListResponse(
    DateTime AsOf,
    int TotalOrders,
    List<PickingLineItem> Lines
);
