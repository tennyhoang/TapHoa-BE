using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.UserHubs.V1.AddFavoriteHub;

public class AddFavoriteHubCommandHandler(
    IRepository<Hub> hubRepo,
    IRepository<UserHub> userHubRepo)
    : IRequestHandler<AddFavoriteHubCommand, Result<FavoriteHubResponse>>
{
    public async Task<Result<FavoriteHubResponse>> Handle(AddFavoriteHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepo.GetByIdAsync(request.HubId);
        if (hub is null)
            return Result<FavoriteHubResponse>.Fail("Không tìm thấy Hub.", "HUB_NOT_FOUND");

        if (hub.Status != HubStatus.Active)
            return Result<FavoriteHubResponse>.Fail("Hub đã ngừng hoạt động.", "HUB_INACTIVE");

        var alreadySaved = await userHubRepo.AnyAsync(
            uh => uh.UserId == request.UserId && uh.HubId == request.HubId);

        if (alreadySaved)
            return Result<FavoriteHubResponse>.Fail("Hub đã có trong danh sách yêu thích.", "HUB_ALREADY_SAVED");

        // Hub đầu tiên được lưu sẽ tự động là mặc định
        var hasAny = await userHubRepo.AnyAsync(uh => uh.UserId == request.UserId);

        var userHub = new UserHub
        {
            UserId    = request.UserId,
            HubId     = request.HubId,
            IsDefault = !hasAny
        };

        await userHubRepo.AddAsync(userHub);
        await userHubRepo.SaveChangesAsync();

        return Result<FavoriteHubResponse>.Ok(new FavoriteHubResponse(
            hub.Id, hub.Name, hub.Address, hub.Ward, hub.District, hub.City,
            hub.Latitude, hub.Longitude, hub.Status, userHub.IsDefault));
    }
}
