using System.ComponentModel.DataAnnotations;
using MediatR;

namespace TapHoa.Application.Cart.V1.AddToCart;

public record AddToCartCommand(
    Guid UserId,
    [Required] Guid ProductId,
    [Required, Range(1, int.MaxValue)] int Quantity
) : IRequest<CartResponse>;
