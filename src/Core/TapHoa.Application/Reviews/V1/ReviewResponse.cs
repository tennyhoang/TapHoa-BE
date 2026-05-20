namespace TapHoa.Application.Reviews.V1;

public record ReviewResponse(
    Guid Id,
    Guid UserId,
    string UserFullName,
    int Rating,
    string? Comment,
    DateTime CreatedAt);
