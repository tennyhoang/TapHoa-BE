using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;
using TapHoa.Persistence.Data;

namespace TapHoa.Persistence.Repositories;

public class HubInventoryRepository(AppDbContext db) : IHubInventoryRepository
{
    private readonly DbSet<HubInventory> _set = db.Set<HubInventory>();

    public IQueryable<HubInventory> Query() => _set.AsQueryable();

    public Task<HubInventory?> FindAsync(Guid hubId, Guid productId) =>
        _set.FirstOrDefaultAsync(hi => hi.HubId == hubId && hi.ProductId == productId);

    public async Task AddAsync(HubInventory entity) => await _set.AddAsync(entity);

    public Task SaveChangesAsync() => db.SaveChangesAsync();
}
