using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1;

public class DriverDispatchCommandHandler(IRepository<Order> orderRepo)
    : IRequestHandler<DriverDispatchCommand, Result<DriverPickupResult>>
{
    public async Task<Result<DriverPickupResult>> Handle(
        DriverDispatchCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderIds.Count == 0)
            return Result<DriverPickupResult>.Fail(
                "Danh sách đơn hàng không được rỗng.", "EMPTY_ORDER_LIST");

        var orders = await orderRepo.Query()
            .Where(o => request.OrderIds.Contains(o.Id) &&
                        o.Status == OrderStatus.Paid_WaitingForBatch)
            .ToListAsync(cancellationToken);

        var errors = new List<string>();
        var dispatched = 0;

        foreach (var id in request.OrderIds)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
            {
                errors.Add($"Đơn {id}: không tìm thấy hoặc không ở trạng thái chờ gom.");
                continue;
            }

            if (order.AssignedDriverId != null && order.AssignedDriverId != request.DriverId)
            {
                errors.Add($"Đơn {id}: đã được gán cho tài xế khác.");
                continue;
            }

            try
            {
                order.StartShipping();
                dispatched++;
            }
            catch (OrderDomainException ex)
            {
                errors.Add($"Đơn {id}: {ex.Message}");
            }
        }

        if (dispatched > 0)
            await orderRepo.SaveChangesAsync();

        return Result<DriverPickupResult>.Ok(new DriverPickupResult(dispatched, errors));
    }
}
