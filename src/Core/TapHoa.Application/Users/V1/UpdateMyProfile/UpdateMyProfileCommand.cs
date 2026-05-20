using MediatR;

namespace TapHoa.Application.Users.V1.UpdateMyProfile;

public record UpdateMyProfileCommand(
    Guid UserId,
    string FullName,
    string? PhoneNumber,
    string? AvatarUrl
) : IRequest<UserProfileResponse>;
