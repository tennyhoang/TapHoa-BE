using TapHoa.Domain.Enums;

namespace TapHoa.Domain.Entities;

public class OrderDamagedReport : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int DamagedQuantity { get; private set; }
    public decimal RefundAmount { get; private set; }
    public string Reason { get; private set; } = default!;
    public DamagedReportStatus Status { get; private set; }
    public Guid ReportedByAgentId { get; private set; }

    public Order Order { get; private set; } = default!;
    public Product Product { get; private set; } = default!;
    public User ReportedByAgent { get; private set; } = default!;

    private OrderDamagedReport() { }

    public static OrderDamagedReport Create(
        Guid orderId,
        Guid productId,
        int damagedQuantity,
        decimal unitPrice,
        string reason,
        Guid agentId)
    {
        return new OrderDamagedReport
        {
            OrderId           = orderId,
            ProductId         = productId,
            DamagedQuantity   = damagedQuantity,
            RefundAmount      = damagedQuantity * unitPrice,
            Reason            = reason.Trim(),
            Status            = DamagedReportStatus.Approved,
            ReportedByAgentId = agentId
        };
    }
}
