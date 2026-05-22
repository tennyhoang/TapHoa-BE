namespace TapHoa.Application.Contracts;

/// <summary>
/// Upload ảnh lên Cloudinary, trả về URL tuyệt đối (https://res.cloudinary.com/...).
/// Interface dùng Stream để Application layer không phụ thuộc ASP.NET Core.
/// </summary>
public interface ICloudinaryService
{
    Task<string> UploadImageAsync(Stream fileStream, string fileName, string folderName = "taphoa_products");
}
