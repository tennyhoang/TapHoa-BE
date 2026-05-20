namespace TapHoa.Api.Endpoints.V1.Upload;

public static class UploadEndpoints
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public static void MapUploadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/upload").WithTags("Upload");

        group.MapPost("/image", async (IFormFile file, IWebHostEnvironment env, HttpContext ctx) =>
        {
            if (file.Length == 0)
                return Results.BadRequest(new { message = "File trống." });

            if (file.Length > MaxFileSize)
                return Results.BadRequest(new { message = "File quá lớn. Tối đa 5MB." });

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
                return Results.BadRequest(new { message = "Định dạng không hỗ trợ. Chỉ chấp nhận jpg, png, webp, gif." });

            var uploadsDir = Path.Combine(env.ContentRootPath, "storage", "uploads");
            Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsDir, fileName);

            await using var stream = File.Create(filePath);
            await file.CopyToAsync(stream);

            var request = ctx.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var url = $"{baseUrl}/storage/uploads/{fileName}";

            return Results.Ok(new { url });
        }).RequireAuthorization("Admin").DisableAntiforgery();
    }
}
