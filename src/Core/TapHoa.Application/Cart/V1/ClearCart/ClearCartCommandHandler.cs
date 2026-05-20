using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Cart.V1.ClearCart;

public class ClearCartCommandHandler(IRepository<CartItem> cartRepo)
    : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var items = await cartRepo.Query()
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
            cartRepo.Remove(item);

        await cartRepo.SaveChangesAsync();
    }
}
