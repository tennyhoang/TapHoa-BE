using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Orders.V1;
using TapHoa.Application.Orders.V1.CreateOrder;
using TapHoa.Application.Orders.V1.Events;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Agent.V1;

public class AgentArriveCommandHandler(
    IRepository<Order> orderRepo,
    IPublisher publisher)
    : IRequestHandler<AgentArriveCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(AgentArriveCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.User)
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<OrderResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        if (order.HubId != request.AgentHubId)
            return Result<OrderResponse>.Fail("Bạn không có quyền thao tác đơn hàng của Hub khác.", "HUB_FORBIDDEN");

        try
        {
            order.MarkInHub();
        }
        catch (OrderDomainException ex)
        {
            return Result<OrderResponse>.Fail(ex.Message, "INVALID_TRANSITION");
        }

        await orderRepo.SaveChangesAsync();

        await publisher.Publish(new OrderArrivedAtHubEvent(
            OrderId:          order.Id,
            UserId:           order.UserId,
            CustomerEmail:    order.User.Email,
            CustomerFullName: order.User.FullName,
            HubId:            order.HubId,
            HubName:          order.Hub.Name,
            HubAddress:       $"{order.Hub.Address}, {order.Hub.Ward}, {order.Hub.District}, {order.Hub.City}",
            ArrivedAt:        order.InHubAt!.Value
        ), cancellationToken);

        return Result<OrderResponse>.Ok(
            CreateOrderCommandHandler.MapToResponse(order, order.Hub));
    }
}
