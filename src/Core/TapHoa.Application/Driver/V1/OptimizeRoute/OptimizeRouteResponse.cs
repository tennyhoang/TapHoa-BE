namespace TapHoa.Application.Driver.V1.OptimizeRoute;

public record OptimizeRouteResponse(
    List<RouteStop> Stops,
    bool            IsOptimized,
    double?         HubLng,
    double?         HubLat
);

public record RouteStop(
    int     StopNumber,
    int     OriginalIndex,
    string  Address,
    double? Lng,
    double? Lat
);
