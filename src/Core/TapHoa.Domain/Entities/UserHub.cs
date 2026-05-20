namespace TapHoa.Domain.Entities;

// Join entity: Customer đăng ký một Hub là điểm nhận hàng yêu thích/mặc định.
// Một Customer có thể lưu nhiều Hub, nhưng chỉ một IsDefault = true tại một thời điểm.
public class UserHub : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid HubId { get; set; }
    public bool IsDefault { get; set; }

    public User User { get; set; } = default!;
    public Hub Hub { get; set; } = default!;
}
