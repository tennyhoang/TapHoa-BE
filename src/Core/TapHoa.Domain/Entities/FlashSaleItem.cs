namespace TapHoa.Domain.Entities;

public class FlashSaleItem : BaseEntity
{
    public Guid FlashSaleSessionId { get; set; }
    public Guid ProductId { get; set; }
    public decimal FlashSalePrice { get; set; }
    public int FlashSaleStock { get; set; }
    public int SoldCount { get; set; }

    public FlashSaleSession Session { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
