namespace TapHoa.Domain.Entities;

public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = default!;
    public int SortOrder { get; set; }
    public Product Product { get; set; } = default!;
}
