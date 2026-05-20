using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.GetMyOrders;

public class GetMyOrdersQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetMyOrdersQuery, Result<PagedResult<OrderResponse>>>
{
    public async Task<Result<PagedResult<OrderResponse>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.UserId == request.UserId);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(o => o.CreatedAt >= request.DateFrom.Value.UtcDateTime);

        if (request.DateTo.HasValue)
            query = query.Where(o => o.CreatedAt <= request.DateTo.Value.UtcDateTime);

        query = request.SortByAmount switch
        {
            "asc"  => query.OrderBy(o => o.TotalAmount),
            "desc" => query.OrderByDescending(o => o.TotalAmount),
            _      => query.OrderByDescending(o => o.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OrderResponse>>.Ok(new PagedResult<OrderResponse>
        {
            Items = items.Select(o => CreateOrder.CreateOrderCommandHandler.MapToResponse(o, o.Hub)).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
