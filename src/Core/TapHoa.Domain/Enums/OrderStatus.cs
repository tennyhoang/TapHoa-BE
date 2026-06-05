namespace TapHoa.Domain.Enums;

public enum OrderStatus
{
    PendingPayment,         // Chờ thanh toán (chuyển khoản)
    Paid_WaitingForBatch,   // Đã thanh toán, chờ gom hàng tại kho
    PackedAtWarehouse,      // Kho đã đóng gói, chờ tài xế lấy hàng
    ShippingToHub,          // Driver đang giao đến Hub
    InHub_ReadyForPickup,   // Hàng đã tại Hub, chờ khách đến lấy
    Completed,              // Khách đã lấy hàng thành công
    Cancelled,
    Refunded                // Admin duyệt khiếu nại → hoàn tiền
}
