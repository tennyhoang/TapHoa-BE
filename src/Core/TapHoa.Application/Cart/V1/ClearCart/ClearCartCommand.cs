using MediatR;

namespace TapHoa.Application.Cart.V1.ClearCart;

public record ClearCartCommand(Guid UserId) : IRequest;
