using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1;

public class GetDriverShippingOrdersQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetDriverShippingOrdersQuery, Result<PagedResult<OrderResponse>>>
{
    public async Task<Result<PagedResult<OrderResponse>>> Handle(
        GetDriverShippingOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Status == OrderStatus.ShippingToHub)
            .OrderByDescending(o => o.ShippingToHubAt);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<OrderResponse>>.Ok(new PagedResult<OrderResponse>
        {
            Items      = items.Select(o => CreateOrder.CreateOrderCommandHandler.MapToResponse(o, o.Hub)).ToList(),
            TotalCount = total,
            Page       = request.Page,
            PageSize   = request.PageSize,
        });
    }
}
