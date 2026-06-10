using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Serilog;
using Scalar.AspNetCore;
using TapHoa.Api.BackgroundJobs;
using TapHoa.Api.Endpoints.V1.Admin;
using TapHoa.Api.Endpoints.V1.Wallet;
using TapHoa.Api.Endpoints.V1.Warehouses;
using TapHoa.Api.Endpoints.V1.Agent;
using TapHoa.Api.Endpoints.V1.Addresses;
using TapHoa.Api.Endpoints.V1.Claims;
using TapHoa.Api.Endpoints.V1.Driver;
using TapHoa.Api.Endpoints.V1.Notifications;
using TapHoa.Api.Endpoints.V1.WarehouseManager;
using TapHoa.Api.Endpoints.V1.Hubs;
using TapHoa.Api.Endpoints.V1.Auth;
using TapHoa.Api.Endpoints.V1.Vouchers;
using TapHoa.Api.Hubs;
using TapHoa.Application.Contracts;
using TapHoa.Api.Endpoints.V1.Cart;
using TapHoa.Api.Endpoints.V1.Categories;
using TapHoa.Api.Endpoints.V1.Orders;
using TapHoa.Api.Endpoints.V1.Payment;
using TapHoa.Api.Endpoints.V1.Articles;
using TapHoa.Api.Endpoints.V1.FlashSale;
using TapHoa.Api.Endpoints.V1.Products;
using TapHoa.Api.Endpoints.V1.Reviews;
using TapHoa.Api.Endpoints.V1.Upload;
using TapHoa.Api.Endpoints.V1.Users;
using TapHoa.Api.Middleware;
using TapHoa.Application;
using TapHoa.Infrastructure;
using TapHoa.Persistence;
using TapHoa.Persistence.Data;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting TapHoa API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Configuration
        .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables();

    builder.Host.UseSerilog((ctx, services, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext());

    builder.Services.AddSignalR();
    builder.Services.AddScoped<IOrderStatusBroadcaster, SignalROrderStatusBroadcaster>();
    builder.Services.AddScoped<IOrderTrackingService, OrderTrackingService>();

    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });

    builder.Services.AddOpenApi();
    builder.Services.AddHttpClient("groq");
    builder.Services.AddHttpClient("pollinations", c => c.Timeout = TimeSpan.FromSeconds(120));
    builder.Services.AddHttpClient("nominatim", c =>
    {
        var contactEmail = builder.Configuration["Nominatim:ContactEmail"];
        c.DefaultRequestHeaders.UserAgent.ParseAdd($"TapHoa/1.0 ({contactEmail})");
        c.Timeout = TimeSpan.FromSeconds(10);
    });
    builder.Services.AddHttpClient("ors", c =>
    {
        c.Timeout = TimeSpan.FromSeconds(30);
    });
    builder.Services.AddHostedService<NightBatchJob>();

    // ── Health Checks ─────────────────────────────────────────────────────────
    var connStr   = builder.Configuration.GetConnectionString("DefaultConnection")!;
    var redisConn = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    builder.Services.AddHealthChecks()
        .AddNpgSql(connStr,       name: "database", tags: ["db",    "ready"])
        .AddRedis(redisConn,      name: "redis",    tags: ["cache", "ready"]);

    // ── OpenTelemetry ─────────────────────────────────────────────────────────
    var otlpEndpoint = builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317";
    var serviceName  = builder.Configuration["OpenTelemetry:ServiceName"] ?? "taphoa-api";
    builder.Services.AddOpenTelemetry()
        .ConfigureResource(r => r.AddService(serviceName))
        .WithTracing(tracing => tracing
            .AddAspNetCoreInstrumentation(o => o.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
        .WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);

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
                    return host == "localhost"
                        || host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("AuthPolicy", opt =>
        {
            opt.PermitLimit = 10;
            opt.Window = TimeSpan.FromMinutes(1);
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db);
    }

    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "storage", "uploads"));

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<AuditLoggingMiddleware>();
    app.UseStaticFiles();
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "storage")),
        RequestPath = "/storage"
    });
    app.UseCors();
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Staging"))
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "TapHoa API";
            options.WithPreferredScheme("Bearer");
            options.WithHttpBearerAuthentication(bearer => bearer.Token = string.Empty);
        }).RequireAuthorization();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    // ── Health Check endpoint ─────────────────────────────────────────────────
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (ctx, report) =>
        {
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsJsonAsync(new
            {
                status       = report.Status.ToString(),
                checks       = report.Entries.Select(e => new
                {
                    name        = e.Key,
                    status      = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    durationMs  = e.Value.Duration.TotalMilliseconds,
                }),
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
            });
        },
        ResultStatusCodes =
        {
            [HealthStatus.Healthy]   = StatusCodes.Status200OK,
            [HealthStatus.Degraded]  = StatusCodes.Status200OK,
            [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
        },
    }).AllowAnonymous();

    // ── SignalR Hub ───────────────────────────────────────────────────────────
    app.MapHub<OrderTrackingHub>("/hubs/order-tracking").RequireAuthorization();

    app.MapArticleEndpoints();
    app.MapFlashSaleEndpoints();
    app.MapAdminFlashSaleEndpoints();
    app.MapAuthEndpoints();
    app.MapProductEndpoints();
    app.MapCartEndpoints();
    app.MapOrderEndpoints();
    app.MapCategoryEndpoints();
    app.MapAddressEndpoints();
    app.MapUserEndpoints();
    app.MapReviewEndpoints();
    app.MapUploadEndpoints();
    app.MapAdminUserEndpoints();
    app.MapAdminRevenueEndpoints();
    app.MapAdminArticleEndpoints();
    app.MapAdminWalletEndpoints();
    app.MapAdminLogisticsEndpoints();
    app.MapHubEndpoints();
    app.MapAgentEndpoints();
    app.MapDriverEndpoints();
    app.MapClaimEndpoints();
    app.MapPaymentEndpoints();
    app.MapWalletEndpoints();
    app.MapWarehouseEndpoints();
    app.MapAdminWarehouseEndpoints();
    app.MapWarehouseManagerEndpoints();
    app.MapNotificationEndpoints();
    app.MapVoucherEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
