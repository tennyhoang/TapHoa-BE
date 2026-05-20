using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;
using TapHoa.Persistence.Data;

namespace TapHoa.Persistence.Repositories;

public class Repository<T>(AppDbContext db) : IRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _set = db.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id) => await _set.FindAsync(id);
    public async Task<List<T>> GetAllAsync() => await _set.ToListAsync();
    public IQueryable<T> Query() => _set.AsQueryable();
    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _set.FirstOrDefaultAsync(predicate);
    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        await _set.AnyAsync(predicate);
    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
