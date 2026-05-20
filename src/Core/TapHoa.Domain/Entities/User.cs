using TapHoa.Domain.Enums;

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
    // Null cho Customer/Admin; set khi Role == Agent
    public Guid? AgentHubId { get; set; }

    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<UserHub> UserHubs { get; set; } = [];
}
