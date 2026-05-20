using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.HubInventories.V1.UpdateHubStock;

public class UpdateHubStockCommandHandler(
    IRepository<Hub> hubRepo,
    IRepository<Product> productRepo,
    IHubInventoryRepository inventoryRepo)
    : IRequestHandler<UpdateHubStockCommand, Result<HubStockResponse>>
{
    public async Task<Result<HubStockResponse>> Handle(UpdateHubStockCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepo.GetByIdAsync(request.HubId);
        if (hub is null)
            return Result<HubStockResponse>.Fail("Không tìm thấy Hub.", "HUB_NOT_FOUND");

        if (hub.Status != HubStatus.Active)
            return Result<HubStockResponse>.Fail("Hub đã ngừng hoạt động.", "HUB_INACTIVE");

        var product = await productRepo.GetByIdAsync(request.ProductId);
        if (product is null)
            return Result<HubStockResponse>.Fail("Không tìm thấy sản phẩm.", "PRODUCT_NOT_FOUND");

        // Upsert: cập nhật nếu đã có, tạo mới nếu chưa có
        var inventory = await inventoryRepo.FindAsync(request.HubId, request.ProductId);
        if (inventory is null)
        {
            inventory = new HubInventory
            {
                HubId     = request.HubId,
                ProductId = request.ProductId,
                Stock     = request.NewStock
            };
            await inventoryRepo.AddAsync(inventory);
        }
        else
        {
            inventory.Stock = request.NewStock;
        }

        await inventoryRepo.SaveChangesAsync();

        return Result<HubStockResponse>.Ok(new HubStockResponse(
            hub.Id, hub.Name, product.Id, product.Name, inventory.Stock));
    }
}
