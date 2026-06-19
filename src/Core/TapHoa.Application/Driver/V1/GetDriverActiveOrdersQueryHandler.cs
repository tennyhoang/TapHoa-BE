using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1;

public class GetDriverActiveOrdersQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetDriverActiveOrdersQuery, Result<List<DriverOrderSummary>>>
{
    public async Task<Result<List<DriverOrderSummary>>> Handle(
        GetDriverActiveOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.User)
            .Include(o => o.Items)
            .Where(o => o.AssignedDriverId == request.DriverId &&
                        o.Status == OrderStatus.PackedAtWarehouse)
            .OrderBy(o => o.CreatedAt)
            .Select(o => new DriverOrderSummary(
                o.Id,
                o.User.FullName,
                o.User.PhoneNumber ?? "",
                o.Items.Count,
                o.TotalAmount,
                o.Hub.Name,
                o.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<DriverOrderSummary>>.Ok(orders);
    }
}
