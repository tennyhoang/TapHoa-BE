namespace TapHoa.Domain.Entities;

public enum WithdrawRequestStatus { Pending, Completed, Rejected }

public class WithdrawRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string BankName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string HolderName { get; set; } = null!;
    public WithdrawRequestStatus Status { get; set; } = WithdrawRequestStatus.Pending;
    public DateTime? ProcessedAt { get; set; }
    public string? AdminNote { get; set; }

    public User User { get; set; } = default!;
}
