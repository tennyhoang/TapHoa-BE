using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;

namespace TapHoa.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public UserRole Role { get; set; } = UserRole.Customer;
    public Guid? AgentHubId { get; set; }
    public decimal WalletBalance { get; private set; } = 0m;

    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<UserHub> UserHubs { get; set; } = [];
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = [];

    public void CreditWallet(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Số tiền cộng ví phải lớn hơn 0.", nameof(amount));
        WalletBalance += amount;
    }

    public void DebitWallet(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Số tiền trừ ví phải lớn hơn 0.", nameof(amount));
        if (WalletBalance < amount)
            throw new OrderDomainException("Số dư ví không đủ để thực hiện giao dịch này.");
        WalletBalance -= amount;
    }
}
