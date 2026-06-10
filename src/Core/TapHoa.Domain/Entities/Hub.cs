using TapHoa.Domain.Enums;

namespace TapHoa.Domain.Entities;

public class Hub : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Ward { get; set; } = default!;
    public string District { get; set; } = default!;
    public string City { get; set; } = default!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public HubStatus Status { get; set; } = HubStatus.Active;

    // BR-013: configurable per Hub/zone
    public decimal MinimumOrderAmount { get; set; } = 50_000m;
    public decimal FreeShippingThreshold { get; set; } = 200_000m;
    public decimal ShippingFee { get; set; } = 15_000m;

    // Các đơn hàng được nhận tại Hub này
    public ICollection<Order> Orders { get; set; } = [];

    // Các Customer đã đánh dấu Hub này là yêu thích/mặc định
    public ICollection<UserHub> UserHubs { get; set; } = [];

    public ICollection<HubInventory> HubInventories { get; set; } = [];
}
