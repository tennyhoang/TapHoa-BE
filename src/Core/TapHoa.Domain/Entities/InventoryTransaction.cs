using TapHoa.Domain.Enums;

namespace TapHoa.Domain.Entities;

public class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? HubId { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? ActorUserId { get; set; }
    public InventoryTransactionType Type { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string Reason { get; set; } = string.Empty;

    public Product Product { get; set; } = default!;
}
