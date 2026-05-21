using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1;

public class DriverPickupFromWarehouseCommandHandler(IRepository<Order> orderRepo)
    : IRequestHandler<DriverPickupFromWarehouseCommand, Result<DriverPickupResult>>
{
    public async Task<Result<DriverPickupResult>> Handle(
        DriverPickupFromWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderIds.Count == 0)
            return Result<DriverPickupResult>.Fail("Danh sách đơn hàng không được rỗng.", "EMPTY_ORDER_LIST");

        var orders = await orderRepo.Query()
            .Where(o => request.OrderIds.Contains(o.Id))
            .ToListAsync(cancellationToken);

        var errors = new List<string>();
        var shipped = 0;

        foreach (var id in request.OrderIds)
        {
            var order = orders.FirstOrDefault(o => o.Id == id);
            if (order is null)
            {
                errors.Add($"Đơn {id}: không tìm thấy.");
                continue;
            }

            try
            {
                order.StartShipping();
                shipped++;
            }
            catch (OrderDomainException ex)
            {
                errors.Add($"Đơn {id}: {ex.Message}");
            }
        }

        if (shipped > 0)
            await orderRepo.SaveChangesAsync();

        return Result<DriverPickupResult>.Ok(new DriverPickupResult(shipped, errors));
    }
}
