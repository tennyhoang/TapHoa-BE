using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.GetMyProfile;

public class GetMyProfileQueryHandler(IRepository<User> userRepo)
    : IRequestHandler<GetMyProfileQuery, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        return MapToResponse(user);
    }

    public static UserProfileResponse MapToResponse(User user) => new(
        user.Id,
        user.FullName,
        user.Email,
        user.PhoneNumber,
        user.AvatarUrl,
        user.IsActive);
}
