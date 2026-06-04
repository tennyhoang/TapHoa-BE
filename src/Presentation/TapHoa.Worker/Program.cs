using MassTransit;
using Serilog;
using TapHoa.Worker.Consumers;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);

    builder.Services.AddSerilog((_, cfg) =>
        cfg.ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext());

    var rabbitHost = builder.Configuration["RabbitMQ:Host"]     ?? "localhost";
    var rabbitUser = builder.Configuration["RabbitMQ:Username"] ?? "guest";
    var rabbitPass = builder.Configuration["RabbitMQ:Password"] ?? "guest";

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<OrderPaidConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(rabbitHost, h =>
            {
                h.Username(rabbitUser);
                h.Password(rabbitPass);
            });
            cfg.ConfigureEndpoints(ctx);
        });
    });

    var host = builder.Build();
    Log.Information("TapHoa Worker starting");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
