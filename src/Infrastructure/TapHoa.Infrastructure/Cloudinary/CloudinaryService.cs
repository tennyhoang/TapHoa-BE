using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using TapHoa.Application.Contracts;

namespace TapHoa.Infrastructure.Cloudinary;

public sealed class CloudinaryService : ICloudinaryService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryService(IOptions<CloudinarySettings> options)
    {
        var s = options.Value;
        var account = new Account(s.CloudName, s.ApiKey, s.ApiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadImageAsync(
        Stream fileStream, string fileName, string folderName = "taphoa_products")
    {
        if (fileStream is null || fileStream.Length == 0)
            throw new ArgumentException("File trống.");

        if (fileStream.Length > MaxFileSize)
            throw new ArgumentException("File quá lớn. Tối đa 5MB.");

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException("Định dạng không hỗ trợ. Chỉ chấp nhận jpg, png, webp, gif.");

        var uploadParams = new ImageUploadParams
        {
            File           = new FileDescription(fileName, fileStream),
            Folder         = folderName,
            UseFilename    = false,
            UniqueFilename = true,
            Overwrite      = false,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto")
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload thất bại: {result.Error.Message}");

        return result.SecureUrl.AbsoluteUri;
    }

    public async Task<string> UploadImageFromUrlAsync(string remoteUrl, string folderName = "taphoa_articles")
    {
        var uploadParams = new ImageUploadParams
        {
            File           = new FileDescription(remoteUrl),
            Folder         = folderName,
            UseFilename    = false,
            UniqueFilename = true,
            Overwrite      = false,
            Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException($"Cloudinary upload thất bại: {result.Error.Message}");

        return result.SecureUrl.AbsoluteUri;
    }
}
