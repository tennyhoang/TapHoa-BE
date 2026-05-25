using MediatR;
using TapHoa.Application.Logistics.V1.DispatchNightBatch;

namespace TapHoa.Api.BackgroundJobs;

/// <summary>
/// BackgroundService tự động chạy DispatchNightBatchCommand lúc 00:00 mỗi đêm.
/// Thay thế hành động bấm tay tại Agent Portal.
/// </summary>
public sealed class NightBatchJob(
    IServiceScopeFactory scopeFactory,
    ILogger<NightBatchJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("NightBatchJob started — next run at {Time}", NextMidnight());

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = NextMidnight() - DateTime.Now;
            if (delay < TimeSpan.Zero)
                delay = TimeSpan.Zero;

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            await RunBatchAsync(stoppingToken);
        }
    }

    private async Task RunBatchAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            var result = await mediator.Send(new DispatchNightBatchCommand(), ct);
            logger.LogInformation(
                "Night batch completed — date={Date} orders={Orders} hubs={Hubs}. Next run at {Next}",
                result.BatchDate, result.TotalOrders, result.TotalHubs, NextMidnight());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Night batch job failed — will retry next midnight");
        }
    }

    private static DateTime NextMidnight() => DateTime.Today.AddDays(1);
}
