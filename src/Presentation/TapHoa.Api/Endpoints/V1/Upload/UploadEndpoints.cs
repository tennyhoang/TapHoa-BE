using Microsoft.Extensions.Configuration;

namespace TapHoa.Api.Endpoints.V1.Upload;

public static class UploadEndpoints
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    // Magic byte signatures for allowed image types
    private static readonly Dictionary<string, byte[]> MagicBytes = new()
    {
        [".jpg"]  = [0xFF, 0xD8, 0xFF],
        [".jpeg"] = [0xFF, 0xD8, 0xFF],
        [".png"]  = [0x89, 0x50, 0x4E, 0x47],
        [".gif"]  = [0x47, 0x49, 0x46, 0x38],
    };

    public static void MapUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/upload").WithTags("Upload");

        group.MapPost("/image", async (IFormFile file, IWebHostEnvironment env, IConfiguration config) =>
        {
            if (file.Length == 0)
                return Results.BadRequest(new { message = "File trống." });

            if (file.Length > MaxFileSize)
                return Results.BadRequest(new { message = "File quá lớn. Tối đa 5MB." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return Results.BadRequest(new { message = "Định dạng không hỗ trợ. Chỉ chấp nhận jpg, png, webp, gif." });

            // Validate actual file content against declared extension (prevent disguised uploads)
            if (!await HasValidMagicBytesAsync(file, ext))
                return Results.BadRequest(new { message = "Nội dung file không khớp định dạng khai báo." });

            var uploadsDir = Path.Combine(env.ContentRootPath, "storage", "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            // Use configured BaseUrl to prevent Host header injection attacks
            var baseUrl = (config["BaseUrl"] ?? throw new InvalidOperationException("BaseUrl chưa được cấu hình."))
                .TrimEnd('/');
            var url = $"{baseUrl}/storage/uploads/{fileName}";

            return Results.Ok(new { url });
        }).RequireAuthorization("Admin").DisableAntiforgery();
    }

    private static async Task<bool> HasValidMagicBytesAsync(IFormFile file, string ext)
    {
        await using var stream = file.OpenReadStream();

        // WebP: RIFF at bytes 0–3, WEBP at bytes 8–11
        if (ext == ".webp")
        {
            var buf = new byte[12];
            if (await stream.ReadAsync(buf) < 12) return false;
            return buf[..4].SequenceEqual("RIFF"u8.ToArray())
                && buf[8..12].SequenceEqual("WEBP"u8.ToArray());
        }

        if (!MagicBytes.TryGetValue(ext, out var expected)) return false;
        var header = new byte[expected.Length];
        var read = await stream.ReadAsync(header);
        return read == expected.Length && header.SequenceEqual(expected);
    }
}
