using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;

namespace TapHoa.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    // Deprecated trong luồng O2O — giữ lại nullable để không mất dữ liệu đơn cũ.
    public Guid? ShippingAddressId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public string? Note { get; set; }
    public string? CancelReason { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippingAt { get; private set; }
    public DateTime? ArrivedAtHubAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    // Hub mà Customer sẽ đến lấy hàng (O2O model)
    public Guid HubId { get; set; }

    public User User { get; set; } = default!;
    public Address? ShippingAddress { get; set; }
    public Hub Hub { get; set; } = default!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderClaim> Claims { get; set; } = [];

    public void Confirm()
    {
        GuardTerminal();
        if (Status != OrderStatus.Pending)
            throw new OrderDomainException($"Không thể xác nhận đơn hàng ở trạng thái '{Status}'.");
        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
    }

    public void Ship()
    {
        GuardTerminal();
        if (Status != OrderStatus.Confirmed)
            throw new OrderDomainException($"Không thể giao hàng khi đơn hàng ở trạng thái '{Status}'.");
        Status = OrderStatus.Shipping;
        ShippingAt = DateTime.UtcNow;
    }

    public void ArriveAtHub()
    {
        GuardTerminal();
        if (Status != OrderStatus.Shipping)
            throw new OrderDomainException($"Chỉ có thể cập nhật 'Đến Hub' khi đơn đang vận chuyển (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.ArrivedAtHub;
        ArrivedAtHubAt = DateTime.UtcNow;
    }

    public void Deliver()
    {
        GuardTerminal();
        if (Status != OrderStatus.ArrivedAtHub)
            throw new OrderDomainException($"Chỉ có thể xác nhận hoàn thành khi hàng đã đến Hub (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        GuardTerminal();
        if (Status != OrderStatus.Pending)
            throw new OrderDomainException("Chỉ có thể hủy đơn hàng đang chờ xử lý.");
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelReason = reason?.Trim();
    }

    // Không gọi GuardTerminal() vì Delivered là điều kiện đầu vào hợp lệ
    public void Refund()
    {
        if (Status != OrderStatus.Delivered)
            throw new OrderDomainException($"Chỉ có thể hoàn tiền đơn hàng đã giao thành công (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }

    private void GuardTerminal()
    {
        if (Status is OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new OrderDomainException($"Đơn hàng đã kết thúc ('{Status}'), không thể cập nhật thêm.");
    }
}
