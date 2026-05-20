using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Cart.V1.UpdateCart;

public class UpdateCartCommandHandler(
    IRepository<CartItem> cartRepo,
    IRepository<Product> productRepo)
    : IRequestHandler<UpdateCartCommand, CartResponse>
{
    public async Task<CartResponse> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
    {
        var item = await cartRepo.FindAsync(c => c.UserId == request.UserId && c.ProductId == request.ProductId)
            ?? throw new KeyNotFoundException("Sản phẩm không có trong giỏ hàng.");

        var product = await productRepo.GetByIdAsync(request.ProductId);
        if (product!.Stock < request.Quantity)
            throw new InvalidOperationException($"Chỉ còn {product.Stock} sản phẩm trong kho.");

        item.Quantity = request.Quantity;
        cartRepo.Update(item);
        await cartRepo.SaveChangesAsync();

        var items = await cartRepo.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        return GetCart.GetCartQueryHandler.MapToResponse(items);
    }
}
