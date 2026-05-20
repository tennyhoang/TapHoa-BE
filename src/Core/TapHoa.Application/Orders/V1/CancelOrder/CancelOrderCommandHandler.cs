using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Exceptions;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.CancelOrder;

public class CancelOrderCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<Product> productRepo,
    IHubInventoryRepository inventoryRepo)
    : IRequestHandler<CancelOrderCommand, Result<OrderResponse>>
{
    public async Task<Result<OrderResponse>> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken);

        if (order is null)
            return Result<OrderResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        try
        {
            order.Cancel(request.CancelReason);
        }
        catch (OrderDomainException ex)
        {
            return Result<OrderResponse>.Fail(ex.Message, "INVALID_TRANSITION");
        }

        foreach (var item in order.Items)
        {
            // Hoàn kho toàn cục
            item.Product.Stock += item.Quantity;
            productRepo.Update(item.Product);

            // Hoàn kho Hub — nếu Hub đang tracking sản phẩm này
            var hubInv = await inventoryRepo.FindAsync(order.HubId, item.ProductId);
            if (hubInv is not null)
                hubInv.Stock += item.Quantity;
        }

        orderRepo.Update(order);
        await orderRepo.SaveChangesAsync();

        return Result<OrderResponse>.Ok(CreateOrder.CreateOrderCommandHandler.MapToResponse(order, order.Hub));
    }
}
