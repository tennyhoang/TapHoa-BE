using MediatR;

namespace TapHoa.Application.Orders.V1.Events;

// Application-level notification: phát sau khi Admin xác nhận hàng đã đến Hub.
// Domain entity (Order.ArriveAtHub) không biết về event này — handler quyết định publish.
public record OrderArrivedAtHubEvent(
    Guid OrderId,
    Guid UserId,
    string CustomerEmail,
    string CustomerFullName,
    Guid HubId,
    string HubName,
    string HubAddress,
    DateTime ArrivedAt
) : INotification;
