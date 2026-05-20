using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.GetOrderById;

public class GetOrderByIdQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse>
{
    public async Task<OrderResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        return CreateOrder.CreateOrderCommandHandler.MapToResponse(order, order.Hub);
    }
}
