namespace TapHoa.Domain.Enums;

public enum InventoryTransactionType
{
    Restock,      // Admin nhập hàng vào kho
    HardReserve,  // Khoá stock khi đơn được xác nhận
    Release,      // Giải phóng stock khi đơn bị huỷ
    Adjustment    // Điều chỉnh thủ công bởi Admin
}
