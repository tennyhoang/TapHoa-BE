namespace TapHoa.Domain.Entities;

public class UserNotification : BaseEntity
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public bool IsRead { get; set; }
    public string? Data { get; set; }

    public User User { get; set; } = default!;
}
