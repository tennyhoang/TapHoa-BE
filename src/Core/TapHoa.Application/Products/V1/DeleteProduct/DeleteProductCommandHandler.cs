using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Products.V1.DeleteProduct;

public class DeleteProductCommandHandler(IRepository<Product> productRepo)
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepo.GetByIdAsync(request.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy sản phẩm.");

        product.IsActive = false;
        productRepo.Update(product);
        await productRepo.SaveChangesAsync();
    }
}
