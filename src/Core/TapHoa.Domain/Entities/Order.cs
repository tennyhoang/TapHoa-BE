using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;

namespace TapHoa.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    // Deprecated trong luồng O2O — giữ lại nullable để không mất dữ liệu đơn cũ.
    public Guid? ShippingAddressId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; private set; } = OrderStatus.PendingPayment;
    public string? Note { get; set; }
    public string? CancelReason { get; private set; }
    public string? PaymentRef { get; set; }          // Mã tham chiếu chuyển khoản, e.g. TH2685894A
    public DateTime? PaidAt { get; private set; }
    public DateTime? ShippingToHubAt { get; private set; }
    public DateTime? InHubAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    // Hub mà Customer sẽ đến lấy hàng (O2O model)
    public Guid HubId { get; set; }

    public User User { get; set; } = default!;
    public Address? ShippingAddress { get; set; }
    public Hub Hub { get; set; } = default!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderClaim> Claims { get; set; } = [];

    // SePay webhook xác nhận chuyển khoản thành công
    public void ConfirmPayment()
    {
        GuardTerminal();
        if (Status != OrderStatus.PendingPayment)
            throw new OrderDomainException($"Đơn hàng không ở trạng thái chờ thanh toán (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.Paid_WaitingForBatch;
        PaidAt = DateTime.UtcNow;
    }

    // Driver nhận lô gom đêm → bắt đầu giao đến Hub
    public void StartShipping()
    {
        GuardTerminal();
        if (Status != OrderStatus.Paid_WaitingForBatch)
            throw new OrderDomainException($"Không thể bắt đầu giao khi đơn hàng ở trạng thái '{Status}'.");
        Status = OrderStatus.ShippingToHub;
        ShippingToHubAt = DateTime.UtcNow;
    }

    // Agent xác nhận đã nhận hàng từ Driver tại Hub
    public void MarkInHub()
    {
        GuardTerminal();
        if (Status != OrderStatus.ShippingToHub)
            throw new OrderDomainException($"Chỉ có thể xác nhận đến Hub khi đơn đang vận chuyển (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.InHub_ReadyForPickup;
        InHubAt = DateTime.UtcNow;
    }

    // Agent xác nhận khách đã đến lấy hàng
    public void Complete()
    {
        GuardTerminal();
        if (Status != OrderStatus.InHub_ReadyForPickup)
            throw new OrderDomainException($"Chỉ có thể hoàn thành khi hàng đã sẵn sàng tại Hub (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel(string? reason = null)
    {
        GuardTerminal();
        if (Status != OrderStatus.PendingPayment && Status != OrderStatus.Paid_WaitingForBatch)
            throw new OrderDomainException("Chỉ có thể hủy đơn hàng đang chờ thanh toán hoặc chờ gom hàng.");
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancelReason = reason?.Trim();
    }

    // Không gọi GuardTerminal() vì Completed là điều kiện đầu vào hợp lệ
    public void Refund()
    {
        if (Status != OrderStatus.Completed)
            throw new OrderDomainException($"Chỉ có thể hoàn tiền đơn hàng đã hoàn thành (trạng thái hiện tại: '{Status}').");
        Status = OrderStatus.Refunded;
        RefundedAt = DateTime.UtcNow;
    }

    private void GuardTerminal()
    {
        if (Status is OrderStatus.Completed or OrderStatus.Cancelled or OrderStatus.Refunded)
            throw new OrderDomainException($"Đơn hàng đã kết thúc ('{Status}'), không thể cập nhật thêm.");
    }
}
