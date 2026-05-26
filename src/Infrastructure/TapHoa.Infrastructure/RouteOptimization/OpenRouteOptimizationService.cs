using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TapHoa.Application.Contracts;

namespace TapHoa.Infrastructure.RouteOptimization;

public sealed class OpenRouteOptimizationService(
    IHttpClientFactory httpClientFactory,
    IConfiguration     configuration)
    : IRouteOptimizationService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private string ApiKey =>
        configuration["OpenRouteService:ApiKey"]
        ?? Environment.GetEnvironmentVariable("ORS_API_KEY")
        ?? "";

    // ── Geocoding (Nominatim OpenStreetMap — miễn phí, không cần API key) ──────

    public async Task<double[]?> GeocodeAsync(string address, CancellationToken ct = default)
    {
        try
        {
            var client  = httpClientFactory.CreateClient("nominatim");
            var encoded = Uri.EscapeDataString(address);
            var url     = $"https://nominatim.openstreetmap.org/search" +
                          $"?q={encoded}&format=json&limit=1&countrycodes=vn";

            using var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc  = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.GetArrayLength() == 0) return null;

            var first = root[0];
            if (!first.TryGetProperty("lon", out var lon) ||
                !first.TryGetProperty("lat", out var lat))
                return null;

            // ORS convention: [longitude, latitude]
            return [double.Parse(lon.GetString()!), double.Parse(lat.GetString()!)];
        }
        catch
        {
            return null;
        }
    }

    // ── Route Optimization (OpenRouteService VRP/TSP) ────────────────────────

    public async Task<int[]> OptimizeAsync(
        double[]             hubCoords,
        IReadOnlyList<double[]> jobCoords,
        CancellationToken    ct = default)
    {
        var fallback = Enumerable.Range(0, jobCoords.Count).ToArray();

        if (string.IsNullOrWhiteSpace(ApiKey) || jobCoords.Count == 0)
            return fallback;

        try
        {
            var body = new
            {
                vehicles = new[]
                {
                    new
                    {
                        id      = 1,
                        profile = "driving-car",
                        start   = hubCoords,   // xuất phát tại Hub
                        end     = hubCoords,   // kết thúc tại Hub
                    }
                },
                jobs = jobCoords
                    .Select((coords, i) => new { id = i, location = coords })
                    .ToArray(),
            };

            var json    = JsonSerializer.Serialize(body);
            var client  = httpClientFactory.CreateClient("ors");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/optimization")
            {
                Headers  = { Authorization = new AuthenticationHeaderValue("Bearer", ApiKey) },
                Content  = new StringContent(json, Encoding.UTF8, "application/json"),
            };

            using var response = await client.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode) return fallback;

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);

            // ORS trả về routes[0].steps — lọc các step có type = "job"
            var steps = doc.RootElement
                .GetProperty("routes")[0]
                .GetProperty("steps")
                .EnumerateArray()
                .Where(s => s.GetProperty("type").GetString() == "job")
                .Select(s => s.GetProperty("id").GetInt32())
                .ToArray();

            return steps.Length > 0 ? steps : fallback;
        }
        catch
        {
            return fallback;
        }
    }
}
