namespace TapHoa.Domain.Entities;

public enum WalletTopupStatus { Pending, Completed }

public class WalletTopupRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentRef { get; set; } = null!;
    public WalletTopupStatus Status { get; set; } = WalletTopupStatus.Pending;
    public DateTime? CompletedAt { get; set; }

    public User User { get; set; } = default!;

    public void Complete()
    {
        Status = WalletTopupStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}
