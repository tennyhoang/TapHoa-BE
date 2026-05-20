using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Cart.V1.GetCart;

public class GetCartQueryHandler(IRepository<CartItem> cartRepo)
    : IRequestHandler<GetCartQuery, CartResponse>
{
    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var items = await cartRepo.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        return MapToResponse(items);
    }

    internal static CartResponse MapToResponse(List<CartItem> items) => new(
        items.Select(c => new CartItemResponse(
            c.ProductId, c.Product.Name, c.Product.ThumbnailUrl,
            c.Product.Price, c.Product.DiscountPrice, c.Quantity, c.Product.Stock
        )).ToList()
    );
}
