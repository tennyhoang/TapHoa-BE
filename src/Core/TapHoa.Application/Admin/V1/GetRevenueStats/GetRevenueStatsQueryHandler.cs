using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Admin.V1.GetRevenueStats;

public class GetRevenueStatsQueryHandler(
    IRepository<Order> orderRepo,
    IRepository<OrderItem> orderItemRepo)
    : IRequestHandler<GetRevenueStatsQuery, Result<RevenueStatsResponse>>
{
    public async Task<Result<RevenueStatsResponse>> Handle(
        GetRevenueStatsQuery request, CancellationToken cancellationToken)
    {
        if (request.TimeRange == TimeRange.CustomRange)
        {
            if (!request.From.HasValue || !request.To.HasValue)
                return Result<RevenueStatsResponse>.Fail(
                    "CustomRange yêu cầu cả From và To.", "INVALID_DATE_RANGE");
            if (request.From > request.To)
                return Result<RevenueStatsResponse>.Fail(
                    "From không được lớn hơn To.", "INVALID_DATE_RANGE");
        }

        var (fromUtc, toUtc) = GetDateRange(request);
        var prevFromUtc = fromUtc - (toUtc - fromUtc);

        // ── Main period aggregates ─────────────────────────────────────────────
        // GroupBy(_ => 1) collapses all rows into one group → EF Core translates
        // to a single SELECT with CASE WHEN conditional sums; no rows loaded.
        var mainStats = await orderRepo.Query()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt < toUtc)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                ActualRevenue    = g.Sum(o => o.Status == OrderStatus.Completed              ? (decimal?)o.TotalAmount : null) ?? 0m,
                ProjectedRevenue = g.Sum(o => o.Status == OrderStatus.Paid_WaitingForBatch
                                           || o.Status == OrderStatus.ShippingToHub
                                           || o.Status == OrderStatus.InHub_ReadyForPickup   ? (decimal?)o.TotalAmount : null) ?? 0m,
                CancelledRevenue = g.Sum(o => o.Status == OrderStatus.Cancelled              ? (decimal?)o.TotalAmount : null) ?? 0m,
                TotalOrders      = g.Count(),
                SuccessfulOrders = g.Count(o => o.Status == OrderStatus.Completed),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var actualRevenue    = mainStats?.ActualRevenue    ?? 0m;
        var projectedRevenue = mainStats?.ProjectedRevenue ?? 0m;
        var cancelledRevenue = mainStats?.CancelledRevenue ?? 0m;
        var totalOrders      = mainStats?.TotalOrders      ?? 0;
        var successfulOrders = mainStats?.SuccessfulOrders ?? 0;

        // ── Previous period (same duration) → GrowthRate ──────────────────────
        var prevRevenue = await orderRepo.Query()
            .Where(o => o.CreatedAt >= prevFromUtc && o.CreatedAt < fromUtc
                     && o.Status == OrderStatus.Completed)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0m;

        decimal? growthRate = prevRevenue == 0
            ? (actualRevenue > 0 ? 100m : null)
            : Math.Round((actualRevenue - prevRevenue) / prevRevenue * 100, 2);

        // ── Chart data: actual + projected revenue per day ────────────────────
        var chartRaw = await orderRepo.Query()
            .Where(o => o.CreatedAt >= fromUtc && o.CreatedAt < toUtc)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new
            {
                Date             = g.Key,
                ActualRevenue    = g.Sum(o => o.Status == OrderStatus.Completed              ? (decimal?)o.TotalAmount : null) ?? 0m,
                ProjectedRevenue = g.Sum(o => o.Status == OrderStatus.Paid_WaitingForBatch
                                           || o.Status == OrderStatus.ShippingToHub
                                           || o.Status == OrderStatus.InHub_ReadyForPickup   ? (decimal?)o.TotalAmount : null) ?? 0m,
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        var chartData = chartRaw
            .Select(x => new RevenueChartPoint(DateOnly.FromDateTime(x.Date), x.ActualRevenue, x.ProjectedRevenue))
            .ToList();

        // ── Top 5 products by quantity sold (Delivered orders only) ───────────
        // Navigates OrderItem → Order and OrderItem → Product entirely in SQL.
        var topProductsRaw = await orderItemRepo.Query()
            .Where(i => i.Order.CreatedAt >= fromUtc && i.Order.CreatedAt < toUtc
                     && i.Order.Status == OrderStatus.Completed)
            .GroupBy(i => new { i.ProductId, ProductName = i.Product.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                TotalQuantity = g.Sum(i => i.Quantity),
                TotalRevenue  = g.Sum(i => i.UnitPrice * i.Quantity),
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topProducts = topProductsRaw
            .Select(x => new TopProductItem(x.ProductId, x.ProductName, x.TotalQuantity, x.TotalRevenue))
            .ToList();

        return Result<RevenueStatsResponse>.Ok(new RevenueStatsResponse(
            actualRevenue, projectedRevenue, cancelledRevenue,
            totalOrders, successfulOrders, growthRate,
            chartData, topProducts));
    }

    private static (DateTime From, DateTime To) GetDateRange(GetRevenueStatsQuery q)
    {
        var today = DateTime.UtcNow.Date;
        return q.TimeRange switch
        {
            TimeRange.Today       => (today, today.AddDays(1)),
            TimeRange.Week        => (today.AddDays(-6), today.AddDays(1)),
            TimeRange.Month       => (today.AddDays(-29), today.AddDays(1)),
            TimeRange.Year        => (new DateTime(today.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), today.AddDays(1)),
            TimeRange.CustomRange => (
                q.From!.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                q.To!.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).AddDays(1)
            ),
            _ => throw new ArgumentException($"Unknown TimeRange: {q.TimeRange}")
        };
    }
}
