using Microsoft.EntityFrameworkCore.Storage;
using TapHoa.Domain.Repositories;

namespace TapHoa.Persistence.Data;

internal sealed class DbContextTransaction(IDbContextTransaction inner) : ITransaction
{
    public Task CommitAsync(CancellationToken ct = default) => inner.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct = default) => inner.RollbackAsync(ct);
    public ValueTask DisposeAsync() => inner.DisposeAsync();
}
