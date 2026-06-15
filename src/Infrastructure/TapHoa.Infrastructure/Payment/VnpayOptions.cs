namespace TapHoa.Infrastructure.Payment;

public class VnpayOptions
{
    public const string Section = "Vnpay";
    public string Url { get; set; } = string.Empty;
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
}
