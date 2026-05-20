using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Admin.V1.GetRevenueStats;

public record GetRevenueStatsQuery(
    TimeRange TimeRange = TimeRange.Month,
    DateOnly? From = null,
    DateOnly? To = null
) : IRequest<Result<RevenueStatsResponse>>;
