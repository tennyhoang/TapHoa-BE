using MediatR;

namespace TapHoa.Application.Products.V1.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest;
