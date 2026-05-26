namespace TapHoa.Application.Contracts;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folderName = "taphoa_products");

    Task<string> UploadImageFromUrlAsync(string remoteUrl, string folderName = "taphoa_articles");
}
