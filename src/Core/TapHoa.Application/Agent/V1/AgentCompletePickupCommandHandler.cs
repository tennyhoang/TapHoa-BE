using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;
using TapHoa.Application.Orders.V1.CreateOrder;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Agent.V1;

public class AgentCompletePickupCommandHandler(IRepository<Order> orderRepo)
    : IRequestHandler<AgentCompletePickupCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(AgentCompletePickupCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<OrderResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        if (order.HubId != request.AgentHubId)
            return Result<OrderResponse>.Fail("Bạn không có quyền thao tác đơn hàng của Hub khác.", "HUB_FORBIDDEN");

        try
        {
            order.Complete();
        }
        catch (OrderDomainException ex)
        {
            return Result<OrderResponse>.Fail(ex.Message, "INVALID_TRANSITION");
        }

        await orderRepo.SaveChangesAsync();

        return Result<OrderResponse>.Ok(
            CreateOrderCommandHandler.MapToResponse(order, order.Hub));
    }
}
