using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Hubs.V1.GetActiveHubs;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Hubs.V1.UpdateHub;

public class UpdateHubCommandHandler(IRepository<Hub> hubRepo)
    : IRequestHandler<UpdateHubCommand, Result<HubResponse>>
{
    public async Task<Result<HubResponse>> Handle(UpdateHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepo.GetByIdAsync(request.Id);
        if (hub is null)
            return Result<HubResponse>.Fail("Không tìm thấy Hub.", "HUB_NOT_FOUND");

        // Kiểm tra trùng tên trong cùng quận/thành phố (bỏ qua chính nó)
        var duplicate = await hubRepo.AnyAsync(h =>
            h.Id != request.Id &&
            h.Name == request.Name.Trim() &&
            h.District == request.District.Trim() &&
            h.City == request.City.Trim());

        if (duplicate)
            return Result<HubResponse>.Fail(
                $"Hub '{request.Name}' đã tồn tại tại {request.District}, {request.City}.",
                "HUB_DUPLICATE");

        hub.Name      = request.Name.Trim();
        hub.Address   = request.Address.Trim();
        hub.Ward      = request.Ward.Trim();
        hub.District  = request.District.Trim();
        hub.City      = request.City.Trim();
        hub.Latitude  = request.Latitude;
        hub.Longitude = request.Longitude;

        // Hub được load bởi GetByIdAsync (FindAsync) → đã tracked → không cần gọi Update()
        await hubRepo.SaveChangesAsync();

        return Result<HubResponse>.Ok(GetActiveHubsQueryHandler.MapToResponse(hub));
    }
}
