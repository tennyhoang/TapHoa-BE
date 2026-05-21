namespace TapHoa.Domain.Enums;

public enum OrderStatus
{
    Paid_WaitingForBatch,   // Đã thanh toán, chờ gom hàng giao đêm
    ShippingToHub,          // Driver đang giao đến Hub
    InHub_ReadyForPickup,   // Hàng đã tại Hub, chờ khách đến lấy
    Completed,              // Khách đã lấy hàng thành công
    Cancelled,
    Refunded                // Admin duyệt khiếu nại → hoàn tiền
}
