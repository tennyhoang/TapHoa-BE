namespace TapHoa.Infrastructure.Cloudinary;

public sealed class CloudinarySettings
{
    public string CloudName { get; init; } = default!;
    public string ApiKey    { get; init; } = default!;
    public string ApiSecret { get; init; } = default!;
}
