namespace TapHoa.Application.Admin.V1.GetRevenueStats;

public record RevenueStatsResponse(
    decimal ActualRevenue,
    decimal ProjectedRevenue,
    decimal CancelledRevenue,
    int TotalOrders,
    int SuccessfulOrders,
    decimal? GrowthRate,
    List<RevenueChartPoint> ChartData,
    List<TopProductItem> TopProducts
);

public record RevenueChartPoint(DateOnly Date, decimal ActualRevenue, decimal ProjectedRevenue);

public record TopProductItem(Guid ProductId, string ProductName, int TotalQuantity, decimal TotalRevenue);
