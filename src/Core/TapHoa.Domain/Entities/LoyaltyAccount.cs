namespace TapHoa.Domain.Entities;

public class LoyaltyAccount : BaseEntity
{
    public Guid UserId { get; set; }
    public int PointsBalance { get; set; }
    public int TotalEarned { get; set; }
    public int TotalRedeemed { get; set; }
    public User User { get; set; } = default!;

    public void Earn(int points)
    {
        if (points <= 0) return;
        PointsBalance += points;
        TotalEarned   += points;
    }

    public void Redeem(int points)
    {
        if (points <= 0) return;
        if (PointsBalance < points)
            throw new InvalidOperationException("Không đủ điểm tích lũy để đổi.");
        PointsBalance   -= points;
        TotalRedeemed   += points;
    }
}
