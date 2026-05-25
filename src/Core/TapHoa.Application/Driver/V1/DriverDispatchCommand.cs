using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1;

/// <summary>
/// Driver xác nhận gom xong lô hàng và bắt đầu vận chuyển đến Hub.
/// Chuyển batch đơn: Paid_WaitingForBatch → ShippingToHub.
/// </summary>
public record DriverDispatchCommand(Guid DriverId, List<Guid> OrderIds)
    : IRequest<Result<DriverPickupResult>>;
