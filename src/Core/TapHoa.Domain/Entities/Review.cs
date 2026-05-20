namespace TapHoa.Domain.Entities;

public class Review : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public User User { get; set; } = default!;
    public Product Product { get; set; } = default!;
}
