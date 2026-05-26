namespace TapHoa.Application.Driver.V1.OptimizeRoute;

public record OptimizeRouteResponse(
    List<RouteStop> Stops,
    bool            IsOptimized   // false = ORS lỗi, trả thứ tự gốc
);

public record RouteStop(
    int    StopNumber,      // 1-based thứ tự ghé
    int    OriginalIndex,   // index gốc trong OrderAddresses
    string Address
);
