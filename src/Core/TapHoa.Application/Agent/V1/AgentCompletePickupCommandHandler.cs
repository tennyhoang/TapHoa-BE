using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Contracts;
using TapHoa.Application.Orders.V1;
using TapHoa.Application.Orders.V1.CreateOrder;
using TapHoa.Application.Orders.V1.Events;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Agent.V1;

public class AgentCompletePickupCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<UserNotification> notificationRepo,
    IOrderStatusBroadcaster broadcaster,
    IPublisher publisher)
    : IRequestHandler<AgentCompletePickupCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(AgentCompletePickupCommand request, CancellationToken cancellationToken)
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
            order.Complete();
        }
        catch (OrderDomainException ex)
        {
            return Result<OrderResponse>.Fail(ex.Message, "INVALID_TRANSITION");
        }

        await orderRepo.SaveChangesAsync();

        // ── SignalR real-time broadcast to customer ────────────────────────────
        await broadcaster.BroadcastStatusChanged(
            order.Id, order.Status.ToString(), order.UserId, cancellationToken);

        // ── Persist in-app notification ───────────────────────────────────────
        var shortRef = order.Id.ToString()[..8].ToUpper();
        await notificationRepo.AddAsync(new UserNotification
        {
            UserId = order.UserId,
            Type   = "OrderStatus",
            Title  = $"Đơn hàng #{shortRef}",
            Body   = "Đơn hàng đã hoàn thành. Cảm ơn bạn đã mua sắm!",
            Data   = System.Text.Json.JsonSerializer.Serialize(new { orderId = order.Id.ToString() }),
        });
        await notificationRepo.SaveChangesAsync();

        // ── Domain event (Expo push + future hooks) ────────────────────────────
        await publisher.Publish(new OrderCompletedEvent(
            OrderId:          order.Id,
            UserId:           order.UserId,
            CustomerEmail:    order.User.Email,
            CustomerFullName: order.User.FullName,
            TotalAmount:      order.TotalAmount,
            CompletedAt:      order.CompletedAt!.Value
        ), cancellationToken);

        return Result<OrderResponse>.Ok(
            CreateOrderCommandHandler.MapToResponse(order, order.Hub));
    }
}
