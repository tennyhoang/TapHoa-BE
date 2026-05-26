using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1.OptimizeRoute;

public class OptimizeRouteCommandHandler(
    IRouteOptimizationService routeService,
    IRepository<User>         userRepo,
    IRepository<Warehouse>    warehouseRepo)
    : IRequestHandler<OptimizeRouteCommand, Result<OptimizeRouteResponse>>
{
    public async Task<Result<OptimizeRouteResponse>> Handle(
        OptimizeRouteCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderAddresses.Count == 0)
            return Result<OptimizeRouteResponse>.Fail("Danh sách đơn hàng rỗng.", "EMPTY_ADDRESSES");

        // ── Resolve kho xuất phát từ tài khoản Driver ────────────────────
        var driver = await userRepo.GetByIdAsync(request.DriverId);
        if (driver is null)
            return Result<OptimizeRouteResponse>.Fail(
                "Không tìm thấy tài khoản tài xế.", "DRIVER_NOT_FOUND");

        if (driver.WarehouseId is null)
            return Result<OptimizeRouteResponse>.Fail(
                "Tài xế chưa được gán kho xuất phát. Liên hệ Admin để được gán kho.",
                "DRIVER_NO_WAREHOUSE");

        var warehouse = await warehouseRepo.GetByIdAsync(driver.WarehouseId.Value);
        if (warehouse is null || !warehouse.IsActive)
            return Result<OptimizeRouteResponse>.Fail(
                "Kho xuất phát không tồn tại hoặc đã bị vô hiệu hoá.",
                "WAREHOUSE_UNAVAILABLE");

        var warehouseAddress =
            $"{warehouse.Address}, {warehouse.Ward}, {warehouse.District}, {warehouse.Province}";

        try
        {
            // ── Step 1: Geocode kho ───────────────────────────────────────
            var hubCoords = await routeService.GeocodeAsync(warehouseAddress, cancellationToken);
            if (hubCoords is null)
                return Fallback(request.OrderAddresses);

            // ── Step 2: Geocode tất cả đơn song song ─────────────────────
            var geocodeTasks = request.OrderAddresses
                .Select(addr => routeService.GeocodeAsync(addr, cancellationToken));

            var geocodeResults = await Task.WhenAll(geocodeTasks);

            // Chỉ giữ lại đơn geocode thành công, kèm index gốc
            var validJobs = geocodeResults
                .Select((coords, i) => (coords, i))
                .Where(x => x.coords is not null)
                .ToList();

            if (validJobs.Count == 0)
                return Fallback(request.OrderAddresses);

            // ── Step 3: Gọi ORS Optimization ─────────────────────────────
            var jobCoords    = validJobs.Select(j => j.coords!).ToList();
            var optimizedIds = await routeService.OptimizeAsync(hubCoords, jobCoords, cancellationToken);

            // ── Step 4: Map kết quả về RouteStop theo thứ tự tối ưu ──────
            var stops = optimizedIds
                .Select((jobId, stopIdx) =>
                {
                    var (coords, originalIndex) = validJobs[jobId];
                    return new RouteStop(
                        StopNumber:    stopIdx + 1,
                        OriginalIndex: originalIndex,
                        Address:       request.OrderAddresses[originalIndex],
                        Lng:           coords![0],
                        Lat:           coords![1]);
                })
                .ToList();

            // Các đơn không geocode được → thêm cuối danh sách (không có tọa độ)
            var optimizedSet = stops.Select(s => s.OriginalIndex).ToHashSet();
            var unresolved = request.OrderAddresses
                .Select((addr, i) => (addr, i))
                .Where(x => !optimizedSet.Contains(x.i))
                .Select((x, idx) => new RouteStop(stops.Count + idx + 1, x.i, x.addr, null, null));

            stops.AddRange(unresolved);

            return Result<OptimizeRouteResponse>.Ok(new OptimizeRouteResponse(
                stops, IsOptimized: true, HubLng: hubCoords[0], HubLat: hubCoords[1]));
        }
        catch
        {
            // Nếu bất kỳ bước nào lỗi → trả thứ tự gốc, không để Driver Portal sập
            return Fallback(request.OrderAddresses);
        }
    }

    private static Result<OptimizeRouteResponse> Fallback(List<string> addresses) =>
        Result<OptimizeRouteResponse>.Ok(new OptimizeRouteResponse(
            Stops: addresses
                .Select((addr, i) => new RouteStop(i + 1, i, addr, null, null))
                .ToList(),
            IsOptimized: false,
            HubLng:      null,
            HubLat:      null));
}
