namespace TapHoa.Domain.Entities;

public class PushToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = default!;
    public string? Platform { get; set; }

    public User User { get; set; } = default!;
}
