using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Cart.V1.AddToCart;

public class AddToCartCommandHandler(
    IRepository<CartItem> cartRepo,
    IRepository<Product> productRepo)
    : IRequestHandler<AddToCartCommand, CartResponse>
{
    public async Task<CartResponse> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(request.ProductId)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        if (!product.IsActive)
            throw new InvalidOperationException("Sản phẩm không còn kinh doanh.");

        var existing = await cartRepo.FindAsync(
            c => c.UserId == request.UserId && c.ProductId == request.ProductId);

        var newQty = (existing?.Quantity ?? 0) + request.Quantity;
        if (product.Stock < newQty)
            throw new InvalidOperationException($"Chỉ còn {product.Stock} sản phẩm trong kho.");

        if (existing is not null)
        {
            existing.Quantity = newQty;
            cartRepo.Update(existing);
        }
        else
        {
            await cartRepo.AddAsync(new CartItem
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        await cartRepo.SaveChangesAsync();

        var items = await cartRepo.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        return GetCart.GetCartQueryHandler.MapToResponse(items);
    }
}
