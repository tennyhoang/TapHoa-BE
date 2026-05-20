using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Cart.V1.RemoveFromCart;

public class RemoveFromCartCommandHandler(IRepository<CartItem> cartRepo)
    : IRequestHandler<RemoveFromCartCommand, CartResponse>
{
    public async Task<CartResponse> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
    {
        var item = await cartRepo.FindAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId)
            ?? throw new KeyNotFoundException("Sản phẩm không có trong giỏ hàng.");

        cartRepo.Remove(item);
        await cartRepo.SaveChangesAsync();

        var items = await cartRepo.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        return GetCart.GetCartQueryHandler.MapToResponse(items);
    }
}
