namespace TapHoa.Application.Users.V1;

public record UserProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string? AvatarUrl,
    bool IsActive);
