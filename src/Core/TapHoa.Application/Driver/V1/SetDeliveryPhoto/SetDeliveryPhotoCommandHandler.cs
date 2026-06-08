using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Driver.V1.SetDeliveryPhoto;

public class SetDeliveryPhotoCommandHandler(IRepository<Order> orderRepo)
    : IRequestHandler<SetDeliveryPhotoCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(SetDeliveryPhotoCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepo.Query()
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result<bool>.Fail("Không tìm thấy đơn hàng.", "ORDER_NOT_FOUND");

        order.SetDeliveryPhoto(request.PhotoUrl);
        await orderRepo.SaveChangesAsync();
        return Result<bool>.Ok(true);
    }
}
