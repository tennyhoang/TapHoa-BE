using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1;

/// <summary>
/// Lấy danh sách đơn hàng đang chờ gom (Paid_WaitingForBatch) theo HubId của Driver.
/// Dùng cho màn hình gom đơn lúc 12h đêm.
/// </summary>
public record GetDriverActiveOrdersQuery(Guid HubId)
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
