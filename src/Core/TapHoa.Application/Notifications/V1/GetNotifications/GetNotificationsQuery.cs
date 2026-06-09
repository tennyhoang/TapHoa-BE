using MediatR;

namespace TapHoa.Application.Notifications.V1.GetNotifications;

public record GetNotificationsQuery(Guid UserId, int Page = 1, int PageSize = 20)
    : IRequest<PagedNotificationsResponse>;
