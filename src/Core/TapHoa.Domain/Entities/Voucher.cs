namespace TapHoa.Domain.Entities;

public class Voucher : BaseEntity
{
    public string Code { get; set; } = default!;
    public string Type { get; set; } = "flat";
    public decimal DiscountValue { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    // BR-014
    public decimal? MaxDiscountAmount { get; set; }
    public bool ExcludeFlashSaleItems { get; set; } = true;
}
