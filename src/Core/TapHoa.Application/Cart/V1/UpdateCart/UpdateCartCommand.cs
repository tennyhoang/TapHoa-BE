using MediatR;

namespace TapHoa.Application.Cart.V1.UpdateCart;

public record UpdateCartCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest<CartResponse>;
