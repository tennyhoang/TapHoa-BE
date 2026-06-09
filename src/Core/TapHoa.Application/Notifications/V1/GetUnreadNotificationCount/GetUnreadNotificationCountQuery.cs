using MediatR;

namespace TapHoa.Application.Notifications.V1.GetUnreadNotificationCount;

public record GetUnreadNotificationCountQuery(Guid UserId) : IRequest<int>;
