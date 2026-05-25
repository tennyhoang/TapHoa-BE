using TapHoa.Domain.Enums;

namespace TapHoa.Domain.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public WalletTransactionType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }

    public User User { get; set; } = default!;
    public Order? Order { get; set; }
}
