namespace TapHoa.Application.Cart.V1;

public record CartItemResponse(
    Guid ProductId,
    string ProductName,
    string? ThumbnailUrl,
    decimal Price,
    decimal? DiscountPrice,
    int Quantity,
    int Stock
)
{
    public decimal UnitPrice => DiscountPrice ?? Price;
    public decimal Subtotal => UnitPrice * Quantity;
}

public record CartResponse(List<CartItemResponse> Items)
{
    public decimal TotalAmount => Items.Sum(i => i.Subtotal);
    public int TotalItems => Items.Sum(i => i.Quantity);
}
