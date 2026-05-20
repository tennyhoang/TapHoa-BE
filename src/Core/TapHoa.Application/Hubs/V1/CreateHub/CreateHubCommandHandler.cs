using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Hubs.V1.GetActiveHubs;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Hubs.V1.CreateHub;

public class CreateHubCommandHandler(IRepository<Hub> hubRepo)
    : IRequestHandler<CreateHubCommand, Result<HubResponse>>
{
    public async Task<Result<HubResponse>> Handle(CreateHubCommand request, CancellationToken cancellationToken)
    {
        var duplicate = await hubRepo.AnyAsync(
            h => h.Name == request.Name && h.District == request.District && h.City == request.City);

        if (duplicate)
            return Result<HubResponse>.Fail(
                $"Hub '{request.Name}' đã tồn tại tại {request.District}, {request.City}.",
                "HUB_DUPLICATE");

        var hub = new Hub
        {
            Name      = request.Name.Trim(),
            Address   = request.Address.Trim(),
            Ward      = request.Ward.Trim(),
            District  = request.District.Trim(),
            City      = request.City.Trim(),
            Latitude  = request.Latitude,
            Longitude = request.Longitude,
        };

        await hubRepo.AddAsync(hub);
        await hubRepo.SaveChangesAsync();

        return Result<HubResponse>.Ok(GetActiveHubsQueryHandler.MapToResponse(hub));
    }
}
