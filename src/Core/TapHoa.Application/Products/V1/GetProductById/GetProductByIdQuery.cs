using MediatR;

namespace TapHoa.Application.Products.V1.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductResponse>;
