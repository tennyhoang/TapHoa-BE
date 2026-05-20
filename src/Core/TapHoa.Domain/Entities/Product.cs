namespace TapHoa.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = default!;
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<OrderItem> OrderItems { get; set; } = [];
    public ICollection<CartItem> CartItems { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<HubInventory> HubInventories { get; set; } = [];
}
