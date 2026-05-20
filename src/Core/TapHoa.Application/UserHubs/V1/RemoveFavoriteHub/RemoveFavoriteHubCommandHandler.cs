using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.UserHubs.V1.RemoveFavoriteHub;

public class RemoveFavoriteHubCommandHandler(IRepository<UserHub> userHubRepo)
    : IRequestHandler<RemoveFavoriteHubCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveFavoriteHubCommand request, CancellationToken cancellationToken)
    {
        var userHub = await userHubRepo.FindAsync(
            uh => uh.UserId == request.UserId && uh.HubId == request.HubId);

        if (userHub is null)
            return Result<bool>.Fail("Hub không có trong danh sách yêu thích.", "HUB_NOT_SAVED");

        var wasDefault = userHub.IsDefault;
        userHubRepo.Remove(userHub);

        // Nếu Hub bị xóa là mặc định, chuyển sang Hub yêu thích cũ nhất còn lại
        if (wasDefault)
        {
            var next = await userHubRepo.Query()
                .Where(uh => uh.UserId == request.UserId && uh.HubId != request.HubId)
                .OrderBy(uh => uh.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (next is not null)
                next.IsDefault = true;
        }

        await userHubRepo.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
