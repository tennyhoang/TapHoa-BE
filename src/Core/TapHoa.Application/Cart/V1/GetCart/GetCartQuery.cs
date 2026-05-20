using MediatR;

namespace TapHoa.Application.Cart.V1.GetCart;

public record GetCartQuery(Guid UserId) : IRequest<CartResponse>;
