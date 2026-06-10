using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Notifications.V1.GetUnreadNotificationCount;

public class GetUnreadNotificationCountQueryHandler(IRepository<UserNotification> repo)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public async Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken ct)
        => await repo.Query().CountAsync(n => n.UserId == request.UserId && !n.IsRead, ct);
}
