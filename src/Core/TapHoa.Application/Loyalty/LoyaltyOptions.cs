namespace TapHoa.Application.Loyalty;

public class LoyaltyOptions
{
    public const string Section = "Loyalty";
    public int EarnPerUnit { get; set; } = 10_000;
    public int RedeemValuePerPoint { get; set; } = 200;
}
