using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;
using TapHoa.Persistence.Data;
using TapHoa.Persistence.Repositories;

namespace TapHoa.Persistence;

public static class PersistenceExtension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IHubInventoryRepository, HubInventoryRepository>();

        return services;
    }
}
