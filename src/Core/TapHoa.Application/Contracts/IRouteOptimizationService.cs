namespace TapHoa.Application.Contracts;

public interface IRouteOptimizationService
{
    /// <summary>
    /// Chuyển địa chỉ tiếng Việt sang tọa độ [longitude, latitude].
    /// Trả null nếu không geocode được.
    /// </summary>
    Task<double[]?> GeocodeAsync(string address, CancellationToken ct = default);

    /// <summary>
    /// Gọi ORS Optimization API để tối ưu thứ tự thăm các job.
    /// Trả về mảng job-id theo thứ tự tối ưu.
    /// Nếu ORS lỗi, trả về thứ tự nguyên gốc [0, 1, 2, ...].
    /// </summary>
    Task<int[]> OptimizeAsync(double[] hubCoords, IReadOnlyList<double[]> jobCoords, CancellationToken ct = default);
}
