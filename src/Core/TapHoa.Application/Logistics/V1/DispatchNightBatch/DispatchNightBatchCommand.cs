using MediatR;

namespace TapHoa.Application.Logistics.V1.DispatchNightBatch;

public record DispatchNightBatchCommand : IRequest<DispatchNightBatchResult>;

public record DispatchNightBatchResult(
    DateOnly BatchDate,
    int TotalOrders,
    int TotalHubs);
