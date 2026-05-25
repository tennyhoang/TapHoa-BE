namespace TapHoa.Infrastructure.Payment;

public class SepayOptions
{
    public const string Section = "SePay";

    /// <summary>
    /// SePay API key gửi trong header Authorization: Apikey {key}
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Shared secret dùng riêng để xác thực header x-sepay-secret-key.
    /// Set qua env var SePay__SecretKey trên Render.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
}
