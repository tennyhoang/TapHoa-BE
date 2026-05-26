namespace TapHoa.Domain.Enums;

public enum UserRole
{
    Customer,
    Admin,
    Agent,            // Nhân viên Hub: xác nhận nhận hàng, bàn giao khách
    Driver,           // Tài xế: giao hàng chặng kho → Hub
    WarehouseManager  // Quản lý kho: quản lý hàng hoá xuất/nhập tại kho
}
