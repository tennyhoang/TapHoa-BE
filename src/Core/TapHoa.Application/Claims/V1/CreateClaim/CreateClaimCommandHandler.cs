using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Claims.V1.CreateClaim;

public class CreateClaimCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<OrderClaim> claimRepo)
    : IRequestHandler<CreateClaimCommand, Result<ClaimResponse>>
{
    public async Task<Result<ClaimResponse>> Handle(CreateClaimCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.FindAsync(
            o => o.Id == request.OrderId && o.UserId == request.CustomerId);

        if (order is null)
            return Result<ClaimResponse>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        if (order.Status != OrderStatus.Completed)
            return Result<ClaimResponse>.Fail(
                "Chỉ có thể khiếu nại đơn hàng đã hoàn thành.", "ORDER_NOT_COMPLETED");

        var existingClaim = await claimRepo.FindAsync(c => c.OrderId == request.OrderId);
        if (existingClaim is not null)
            return Result<ClaimResponse>.Fail(
                "Đơn hàng này đã có khiếu nại đang xử lý.", "CLAIM_ALREADY_EXISTS");

        var claim = new OrderClaim
        {
            OrderId    = request.OrderId,
            CustomerId = request.CustomerId,
            Reason     = request.Reason.Trim(),
            ImageUrl   = request.ImageUrl?.Trim(),
            Status     = ClaimStatus.Pending
        };

        await claimRepo.AddAsync(claim);
        await claimRepo.SaveChangesAsync();

        return Result<ClaimResponse>.Ok(MapToResponse(claim, ""));
    }

    internal static ClaimResponse MapToResponse(OrderClaim c, string customerName) =>
        new(c.Id, c.OrderId, customerName, c.Reason, c.ImageUrl, c.Status, c.CreatedAt);
}
