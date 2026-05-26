namespace TapHoa.Domain.Entities;

public class FlashSaleSession : BaseEntity
{
    public string Name { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<FlashSaleItem> Items { get; set; } = [];
}
