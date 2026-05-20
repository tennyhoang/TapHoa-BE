using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Logistics.V1.GetPickingList;

public class GetPickingListQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetPickingListQuery, PickingListResponse>
{
    public async Task<PickingListResponse> Handle(GetPickingListQuery request, CancellationToken cancellationToken)
    {
        var targetDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var orders = await orderRepo.Query()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Status == request.Status &&
                        DateOnly.FromDateTime(o.CreatedAt) == targetDate)
            .ToListAsync(cancellationToken);

        var lines = orders
            .SelectMany(o => o.Items.Select(i => new
            {
                OrderId     = o.Id,
                i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                i.Quantity
            }))
            .GroupBy(x => new { x.ProductId, x.ProductName })
            .Select(g => new PickingLineItem(
                g.Key.ProductId,
                g.Key.ProductName,
                g.Sum(x => x.Quantity),
                g.Select(x => x.OrderId).Distinct().Count()
            ))
            .OrderBy(l => l.ProductName)
            .ToList();

        return new PickingListResponse(DateTime.UtcNow, orders.Count, lines);
    }
}
