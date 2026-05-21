using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;
using TapHoa.Persistence.Data;
using TapHoa.Persistence.Repositories;

namespace TapHoa.Persistence;

public static class PersistenceExtension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("DefaultConnection")!;
        var connStr = ToNpgsqlConnectionString(raw);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connStr,
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IHubInventoryRepository, HubInventoryRepository>();

        return services;
    }

    // Converts postgresql://user:pass@host/db?sslmode=require → Npgsql key-value format.
    // Passes through unchanged if already in key-value format.
    private static string ToNpgsqlConnectionString(string connStr)
    {
        if (!connStr.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
            !connStr.StartsWith("postgres://",   StringComparison.OrdinalIgnoreCase))
            return connStr;

        var uri      = new Uri(connStr);
        var userInfo = uri.UserInfo.Split(':', 2);

        return new NpgsqlConnectionStringBuilder
        {
            Host                  = uri.Host,
            Port                  = uri.Port > 0 ? uri.Port : 5432,
            Database              = uri.AbsolutePath.TrimStart('/'),
            Username              = Uri.UnescapeDataString(userInfo[0]),
            Password              = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode               = SslMode.Require,
            TrustServerCertificate = true,
        }.ToString();
    }
}
