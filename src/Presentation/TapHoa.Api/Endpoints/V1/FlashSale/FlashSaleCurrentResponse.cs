namespace TapHoa.Api.Endpoints.V1.FlashSale;

public sealed record FlashSaleCurrentResponse(
    Guid SessionId,
    string Name,
    DateTime StartTime,
    DateTime EndTime,
    List<FlashSaleProductResponse> Products
);

public sealed record FlashSaleProductResponse(
    Guid Id,
    string Name,
    string? ThumbnailUrl,
    string CategoryName,
    decimal OriginalPrice,
    decimal FlashSalePrice,
    int FlashSaleStock,
    int SoldCount,
    int StockRemaining
);
