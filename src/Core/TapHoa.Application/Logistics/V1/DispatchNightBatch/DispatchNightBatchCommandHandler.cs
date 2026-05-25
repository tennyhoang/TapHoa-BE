using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Logistics.V1.DispatchNightBatch;

public class DispatchNightBatchCommandHandler(
    IRepository<Order> orderRepo,
    ILogger<DispatchNightBatchCommandHandler> logger)
    : IRequestHandler<DispatchNightBatchCommand, DispatchNightBatchResult>
{
    public async Task<DispatchNightBatchResult> Handle(
        DispatchNightBatchCommand request,
        CancellationToken cancellationToken)
    {
        var batchDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var orders = await orderRepo.Query()
            .Where(o => o.Status == OrderStatus.Paid_WaitingForBatch
                     && DateOnly.FromDateTime(o.CreatedAt) == batchDate)
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            logger.LogInformation("Night batch {Date}: no eligible orders found", batchDate);
            return new DispatchNightBatchResult(batchDate, 0, 0);
        }

        foreach (var order in orders)
            order.StartShipping();

        await orderRepo.SaveChangesAsync();

        var hubCount = orders.Select(o => o.HubId).Distinct().Count();

        logger.LogInformation(
            "Night batch {Date}: dispatched {Orders} orders across {Hubs} hubs",
            batchDate, orders.Count, hubCount);

        return new DispatchNightBatchResult(batchDate, orders.Count, hubCount);
    }
}
