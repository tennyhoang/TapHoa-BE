using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Agent.V1;

public class GetAgentOrdersQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetAgentOrdersQuery, Result<PagedResult<OrderResponse>>>
{
    public async Task<Result<PagedResult<OrderResponse>>> Handle(
        GetAgentOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.HubId == request.HubId);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OrderResponse>>.Ok(new PagedResult<OrderResponse>
        {
            Items = items.Select(o =>
                Orders.V1.CreateOrder.CreateOrderCommandHandler.MapToResponse(o, o.Hub)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
