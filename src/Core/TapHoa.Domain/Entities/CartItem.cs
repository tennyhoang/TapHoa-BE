namespace TapHoa.Domain.Entities;

public class CartItem : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    public User User { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
