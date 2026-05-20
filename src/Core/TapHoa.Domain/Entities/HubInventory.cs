namespace TapHoa.Domain.Entities;

public class HubInventory
{
    public Guid ProductId { get; set; }
    public Guid HubId { get; set; }
    public int Stock { get; set; }

    public Product Product { get; set; } = default!;
    public Hub Hub { get; set; } = default!;
}
