using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Cart.V1.GetCart;

public class GetCartQueryHandler(
    IRepository<CartItem> cartRepo,
    IFlashSaleRepository flashSaleRepo)
    : IRequestHandler<GetCartQuery, CartResponse>
{
    public async Task<CartResponse> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var items = await cartRepo.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var productIds = items.Select(c => c.ProductId).ToList();
        var flashSaleItems = await flashSaleRepo.GetActiveByProductIdsAsync(productIds, cancellationToken);
        var flashSalePrices = flashSaleItems.ToDictionary(f => f.ProductId, f => f.FlashSalePrice);

        return MapToResponse(items, flashSalePrices);
    }

    internal static CartResponse MapToResponse(List<CartItem> items,
        IReadOnlyDictionary<Guid, decimal>? flashSalePrices = null) => new(
        items.Select(c =>
        {
            var fsPrice = flashSalePrices?.GetValueOrDefault(c.ProductId);
            return new CartItemResponse(
                c.ProductId, c.Product.Name, c.Product.ThumbnailUrl,
                c.Product.Price, c.Product.DiscountPrice, c.Quantity, c.Product.Stock
            ) { FlashSalePrice = fsPrice };
        }).ToList()
    );
}
