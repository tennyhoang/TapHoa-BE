namespace TapHoa.Infrastructure.Payment;

public class MomoOptions
{
    public const string Section = "Momo";
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    // Production: https://payment.momo.vn — set via env var Momo__ApiUrl
    public string ApiUrl { get; set; } = "https://test-payment.momo.vn";
}
