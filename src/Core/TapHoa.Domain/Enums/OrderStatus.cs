namespace TapHoa.Domain.Enums;

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipping,
    ArrivedAtHub,  // Hàng đã đến Hub, đang chờ khách ra lấy
    Delivered,
    Cancelled,
    Refunded       // Admin duyệt khiếu nại → hoàn tiền
}
