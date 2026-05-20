using MediatR;

namespace TapHoa.Application.Cart.V1.RemoveFromCart;

public record RemoveFromCartCommand(Guid UserId, Guid ProductId) : IRequest<CartResponse>;
