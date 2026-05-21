using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using NLog;
using NLog.Web;
using Scalar.AspNetCore;
using TapHoa.Api.Endpoints.V1.Admin;
using TapHoa.Api.Endpoints.V1.Agent;
using TapHoa.Api.Endpoints.V1.Addresses;
using TapHoa.Api.Endpoints.V1.Claims;
using TapHoa.Api.Endpoints.V1.Driver;
using TapHoa.Api.Endpoints.V1.Hubs;
using TapHoa.Api.Endpoints.V1.Auth;
using TapHoa.Api.Endpoints.V1.Cart;
using TapHoa.Api.Endpoints.V1.Categories;
using TapHoa.Api.Endpoints.V1.Orders;
using TapHoa.Api.Endpoints.V1.Products;
using TapHoa.Api.Endpoints.V1.Reviews;
using TapHoa.Api.Endpoints.V1.Upload;
using TapHoa.Api.Endpoints.V1.Users;
using TapHoa.Api.Middleware;
using TapHoa.Application;
using TapHoa.Infrastructure;
using TapHoa.Persistence;
using TapHoa.Persistence.Data;

var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();

try
{
    logger.Info("Starting TapHoa API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile("config/appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    builder.Services.AddOpenApi();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);

    // Serialize/deserialize enums as strings so the frontend receives "Pending" not 0.
    builder.Services.ConfigureHttpJsonOptions(opt =>
        opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    var host = new Uri(origin).Host;
                    // localhost (mọi port) cho môi trường dev
                    // *.vercel.app cho frontend production trên Vercel
                    return host == "localhost"
                        || host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    var app = builder.Build();

    // Tự động apply pending migrations và seed dữ liệu mẫu khi DB còn trống.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db);
    }

    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "storage", "uploads"));

    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "storage")),
        RequestPath = "/storage"
    });
    app.UseCors();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "TapHoa API";
            options.WithPreferredScheme("Bearer");
            options.WithHttpBearerAuthentication(bearer => bearer.Token = string.Empty);
        });
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapAuthEndpoints();
    app.MapProductEndpoints();
    app.MapCartEndpoints();
    app.MapOrderEndpoints();
    app.MapCategoryEndpoints();
    app.MapAddressEndpoints();
    app.MapUserEndpoints();
    app.MapReviewEndpoints();
    app.MapUploadEndpoints();
    app.MapAdminRevenueEndpoints();
    app.MapAdminLogisticsEndpoints();
    app.MapHubEndpoints();
    app.MapAgentEndpoints();
    app.MapDriverEndpoints();
    app.MapClaimEndpoints();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}
