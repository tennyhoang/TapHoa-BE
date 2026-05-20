using TapHoa.Domain.Entities;

namespace TapHoa.Domain.Repositories;

public interface IHubInventoryRepository
{
    IQueryable<HubInventory> Query();
    Task<HubInventory?> FindAsync(Guid hubId, Guid productId);
    Task AddAsync(HubInventory entity);
    Task SaveChangesAsync();
}
