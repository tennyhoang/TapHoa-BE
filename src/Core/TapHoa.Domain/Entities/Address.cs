namespace TapHoa.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public string ReceiverName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Province { get; set; } = default!;
    public string District { get; set; } = default!;
    public string Ward { get; set; } = default!;
    public string StreetAddress { get; set; } = default!;
    public bool IsDefault { get; set; }

    public User User { get; set; } = default!;
}
