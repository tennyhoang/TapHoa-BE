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
    IOrderTrackingService orderTracking,
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

        // Hoàn kho Hub khi Admin hủy đơn
        if (order.Status == OrderStatus.Cancelled)
        {
            foreach (var item in order.Items)
            {
                var hubInv = await inventoryRepo.FindAsync(order.HubId, item.ProductId);
                if (hubInv is not null)
                    hubInv.Stock += item.Quantity;
            }
        }

        // Entity đã được tracked — change tracker tự phát hiện mutation, không cần gọi Update().
        await orderRepo.SaveChangesAsync();

        // Notify real-time via SignalR for all status changes
        await orderTracking.NotifyOrderStatusChangedAsync(
            order.UserId, order.Id, order.Status.ToString(), cancellationToken);

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
