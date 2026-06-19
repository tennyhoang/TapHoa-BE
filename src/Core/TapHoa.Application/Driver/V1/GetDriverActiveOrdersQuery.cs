using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1;

/// <summary>
/// Lấy danh sách đơn hàng đã được gán cho tài xế và đang chờ nhận (PackedAtWarehouse).
/// </summary>
public record GetDriverActiveOrdersQuery(Guid DriverId)
    : IRequest<Result<List<DriverOrderSummary>>>;

public record DriverOrderSummary(
    Guid   OrderId,
    string CustomerName,
    string CustomerPhone,
    int    ItemCount,
    decimal TotalAmount,
    string HubName,
    DateTime CreatedAt
);
