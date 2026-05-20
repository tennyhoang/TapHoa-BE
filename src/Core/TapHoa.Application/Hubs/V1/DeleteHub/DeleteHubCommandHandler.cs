using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Hubs.V1.DeleteHub;

public class DeleteHubCommandHandler(
    IRepository<Hub> hubRepo,
    IRepository<Order> orderRepo)
    : IRequestHandler<DeleteHubCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteHubCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepo.GetByIdAsync(request.Id);
        if (hub is null)
            return Result<bool>.Fail("Không tìm thấy Hub.", "HUB_NOT_FOUND");

        // Guard: không xóa Hub còn đơn hàng chưa hoàn tất (tránh FK violation + logic sai)
        var hasActiveOrders = await orderRepo.AnyAsync(o =>
            o.HubId == request.Id &&
            o.Status != OrderStatus.Delivered &&
            o.Status != OrderStatus.Cancelled);

        if (hasActiveOrders)
            return Result<bool>.Fail(
                "Không thể xóa Hub đang có đơn hàng chưa hoàn tất.",
                "HUB_HAS_ACTIVE_ORDERS");

        hubRepo.Remove(hub);
        await hubRepo.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }
}
