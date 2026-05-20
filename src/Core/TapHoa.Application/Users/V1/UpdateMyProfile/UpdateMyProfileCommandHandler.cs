using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Users.V1.UpdateMyProfile;

public class UpdateMyProfileCommandHandler(IRepository<User> userRepo)
    : IRequestHandler<UpdateMyProfileCommand, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.AvatarUrl = request.AvatarUrl;

        userRepo.Update(user);
        await userRepo.SaveChangesAsync();

        return GetMyProfile.GetMyProfileQueryHandler.MapToResponse(user);
    }
}
