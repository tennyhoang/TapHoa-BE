namespace TapHoa.Domain.Entities;

public class LoyaltyTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = "Earned"; // "Earned" | "Redeemed"
    public int Points { get; set; }
    public Guid? OrderId { get; set; }
    public string Description { get; set; } = default!;
}
