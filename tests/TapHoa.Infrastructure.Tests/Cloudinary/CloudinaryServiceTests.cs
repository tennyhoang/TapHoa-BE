using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TapHoa.Infrastructure.Cloudinary;

namespace TapHoa.Infrastructure.Tests.Cloudinary;

public class CloudinaryServiceTests
{
    [Fact]
    public void Constructor_WithValidSettings_DoesNotThrow()
    {
        var options = Options.Create(new CloudinarySettings
        {
            CloudName = "test",
            ApiKey = "test",
            ApiSecret = "test",
        });

        var act = () => new CloudinaryService(options);

        act.Should().NotThrow();
    }

    [Fact]
    public async Task UploadImageAsync_NullStream_ThrowsArgumentException()
    {
        var service = CreateValidService();

        var act = async () => await service.UploadImageAsync(null!, "test.jpg");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File trống.");
    }

    [Fact]
    public async Task UploadImageAsync_EmptyStream_ThrowsArgumentException()
    {
        var service = CreateValidService();

        var act = async () => await service.UploadImageAsync(new MemoryStream(), "test.jpg");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("File trống.");
    }

    [Fact]
    public async Task UploadImageAsync_TooLarge_ThrowsArgumentException()
    {
        var service = CreateValidService();
        var oversized = new MemoryStream(new byte[6 * 1024 * 1024]); // > 5 MB

        var act = async () => await service.UploadImageAsync(oversized, "test.jpg");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*5MB*");
    }

    [Fact]
    public async Task UploadImageAsync_InvalidExtension_ThrowsArgumentException()
    {
        var service = CreateValidService();
        var stream = new MemoryStream(new byte[100]);

        var act = async () => await service.UploadImageAsync(stream, "test.bmp");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Định dạng không hỗ trợ*");
    }

    [Fact]
    public async Task UploadImageAsync_AllowedExtensions_DoNotThrow()
    {
        var service = CreateValidService();
        var stream = new MemoryStream(new byte[100]);

        foreach (var ext in new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" })
        {
            var act = async () => await service.UploadImageAsync(stream, $"test{ext}");
            // Will throw due to Cloudinary API call (no real account), but NOT due to validation
            var exception = await Record.ExceptionAsync(act);
            exception.Should().NotBeOfType<ArgumentException>();
        }
    }

    private static CloudinaryService CreateValidService()
    {
        var options = Options.Create(new CloudinarySettings
        {
            CloudName = "test",
            ApiKey = "test",
            ApiSecret = "test",
        });
        return new CloudinaryService(options);
    }
}
