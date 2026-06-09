using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Notifications.V1.GetNotifications;

public class GetNotificationsQueryHandler(IRepository<UserNotification> repo)
    : IRequestHandler<GetNotificationsQuery, PagedNotificationsResponse>
{
    public async Task<PagedNotificationsResponse> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        var query = repo.Query()
            .Where(n => n.UserId == request.UserId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedNotificationsResponse(
            items.Select(n => new NotificationResponse(
                n.Id, n.Type, n.Title, n.Body, n.IsRead, n.CreatedAt, n.Data
            )).ToList(),
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize)
        );
    }
}
