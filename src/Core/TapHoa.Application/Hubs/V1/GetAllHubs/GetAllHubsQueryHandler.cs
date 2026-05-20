using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Hubs.V1.GetActiveHubs;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Hubs.V1.GetAllHubs;

public class GetAllHubsQueryHandler(IRepository<Hub> hubRepo)
    : IRequestHandler<GetAllHubsQuery, PagedResult<HubResponse>>
{
    public async Task<PagedResult<HubResponse>> Handle(GetAllHubsQuery request, CancellationToken cancellationToken)
    {
        var query = hubRepo.Query().AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(h => h.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(h => h.City.ToLower().Contains(request.City.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.District))
            query = query.Where(h => h.District.ToLower().Contains(request.District.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(h => h.Name.ToLower().Contains(request.Search.ToLower()));

        query = query
            .OrderBy(h => h.City)
            .ThenBy(h => h.District)
            .ThenBy(h => h.Name);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<HubResponse>
        {
            Items      = items.Select(GetActiveHubsQueryHandler.MapToResponse).ToList(),
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize
        };
    }
}
