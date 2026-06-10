using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Application.Orders.V1.Events;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(
    IRepository<Order> orderRepo,
    IHubInventoryRepository inventoryRepo,
    IRepository<UserNotification> notificationRepo,
    IRepository<InventoryTransaction> inventoryTxRepo,
    IOrderStatusBroadcaster broadcaster,
    IPublisher publisher)
    : IRequestHandler<UpdateOrderStatusCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.User)
            .Include(o => o.Hub)
            .Include(o => o.ShippingAddress)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<OrderResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        try
        {
            switch (request.Status)
            {
                case OrderStatus.ShippingToHub:        order.StartShipping();              break;
                case OrderStatus.InHub_ReadyForPickup: order.MarkInHub();                  break;
                case OrderStatus.Completed:            order.Complete();                   break;
                case OrderStatus.Cancelled:            order.Cancel(request.CancelReason); break;
                default:
                    return Result<OrderResponse>.Fail(
                        $"Trạng thái '{request.Status}' không hợp lệ.", "INVALID_STATUS");
            }
        }
        catch (OrderDomainException ex)
        {
            return Result<OrderResponse>.Fail(ex.Message, "INVALID_TRANSITION");
        }

        // Hoàn kho Hub khi Admin hủy đơn + audit trail (BR-012)
        if (order.Status == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var hubInv = await inventoryRepo.FindAsync(order.HubId, item.ProductId);
                if (hubInv is not null)
                {
                    var before = hubInv.Stock;
                    hubInv.Stock += item.Quantity;
                    await inventoryTxRepo.AddAsync(new InventoryTransaction
                    {
                        ProductId      = item.ProductId,
                        HubId          = order.HubId,
                        OrderId        = order.Id,
                        Type           = InventoryTransactionType.Release,
                        QuantityBefore = before,
                        QuantityAfter  = hubInv.Stock,
                        Reason         = $"Huỷ đơn #{order.Id.ToString()[..8].ToUpper()}",
                    });
                }
            }
            await inventoryTxRepo.SaveChangesAsync();
        }

        // Entity đã được tracked — change tracker tự phát hiện mutation, không cần gọi Update().
        await orderRepo.SaveChangesAsync();

        // ── SignalR broadcast ──────────────────────────────────────────────────
        await broadcaster.BroadcastStatusChanged(
            order.Id, order.Status.ToString(), order.UserId, cancellationToken);

        // ── Create notification ────────────────────────────────────────────────
        var shortRef = order.Id.ToString()[..8].ToUpper();
        var (title, body) = order.Status switch
        {
            OrderStatus.ShippingToHub        => ($"Đơn hàng #{shortRef}", "Đơn hàng đang được vận chuyển đến điểm nhận."),
            OrderStatus.InHub_ReadyForPickup => ($"Đơn hàng #{shortRef}", "Hàng đã đến điểm nhận, bạn có thể đến lấy."),
            OrderStatus.Completed            => ($"Đơn hàng #{shortRef}", "Đơn hàng đã hoàn thành. Cảm ơn bạn đã mua sắm!"),
            OrderStatus.Cancelled            => ($"Đơn hàng #{shortRef}", $"Đơn hàng đã bị hủy. {order.CancelReason}"),
            _                                => ($"Đơn hàng #{shortRef}", $"Trạng thái đơn hàng đã thay đổi thành {order.Status}."),
        };

        await notificationRepo.AddAsync(new UserNotification
        {
            UserId  = order.UserId,
            Type    = "OrderStatus",
            Title   = title,
            Body    = body,
            Data    = System.Text.Json.JsonSerializer.Serialize(new { orderId = order.Id.ToString() }),
        });
        await notificationRepo.SaveChangesAsync();

        if (order.Status == OrderStatus.InHub_ReadyForPickup)
        {
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
        }

        if (order.Status == OrderStatus.Completed)
        {
            await publisher.Publish(new OrderCompletedEvent(
                OrderId:     order.Id,
                UserId:      order.UserId,
                TotalAmount: order.TotalAmount,
                CompletedAt: order.CompletedAt!.Value
            ), cancellationToken);
        }

        return Result<OrderResponse>.Ok(
            CreateOrder.CreateOrderCommandHandler.MapToResponse(order, order.Hub));
    }
}
