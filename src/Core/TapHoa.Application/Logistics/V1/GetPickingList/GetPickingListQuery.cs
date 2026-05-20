using MediatR;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Logistics.V1.GetPickingList;

// Mặc định lấy đơn Confirmed của hôm nay — Admin có thể truyền khác nếu cần.
public record GetPickingListQuery(
    OrderStatus Status = OrderStatus.Confirmed,
    DateOnly? Date = null
) : IRequest<PickingListResponse>;
