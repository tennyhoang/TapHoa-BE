using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1;

public class GetDriverOrdersQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetDriverOrdersQuery, Result<List<DriverHubBatch>>>
{
    public async Task<Result<List<DriverHubBatch>>> Handle(
        GetDriverOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Where(o => o.Status == OrderStatus.Paid_WaitingForBatch)
            .OrderBy(o => o.Hub.Name).ThenBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        var batches = orders
            .GroupBy(o => o.Hub)
            .Select(g => new DriverHubBatch(
                HubId:          g.Key.Id,
                HubName:        g.Key.Name,
                HubFullAddress: $"{g.Key.Address}, {g.Key.Ward}, {g.Key.District}, {g.Key.City}",
                OrderCount:     g.Count(),
                TotalAmount:    g.Sum(o => o.TotalAmount),
                Orders:         g.Select(o => Orders.V1.CreateOrder.CreateOrderCommandHandler
                                    .MapToResponse(o, o.Hub)).ToList()
            ))
            .ToList();

        return Result<List<DriverHubBatch>>.Ok(batches);
    }
}
