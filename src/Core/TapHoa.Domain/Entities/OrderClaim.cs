using TapHoa.Domain.Enums;

namespace TapHoa.Domain.Entities;

public class OrderClaim : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string Reason { get; set; } = default!;
    public string? ImageUrl { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    public Order Order { get; set; } = default!;
    public User Customer { get; set; } = default!;
}
