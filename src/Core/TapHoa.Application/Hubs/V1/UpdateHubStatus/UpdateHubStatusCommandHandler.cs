using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Hubs.V1.GetActiveHubs;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Hubs.V1.UpdateHubStatus;

public class UpdateHubStatusCommandHandler(IRepository<Hub> hubRepo)
    : IRequestHandler<UpdateHubStatusCommand, Result<HubResponse>>
{
    public async Task<Result<HubResponse>> Handle(UpdateHubStatusCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepo.GetByIdAsync(request.HubId);

        if (hub is null)
            return Result<HubResponse>.Fail("Không tìm thấy Hub.", "HUB_NOT_FOUND");

        if (hub.Status == request.Status)
            return Result<HubResponse>.Fail(
                $"Hub đã ở trạng thái '{request.Status}'.", "HUB_STATUS_UNCHANGED");

        hub.Status = request.Status;

        await hubRepo.SaveChangesAsync();

        return Result<HubResponse>.Ok(GetActiveHubsQueryHandler.MapToResponse(hub));
    }
}
