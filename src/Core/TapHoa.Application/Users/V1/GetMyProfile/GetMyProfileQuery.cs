using MediatR;

namespace TapHoa.Application.Users.V1.GetMyProfile;

public record GetMyProfileQuery(Guid UserId) : IRequest<UserProfileResponse>;
