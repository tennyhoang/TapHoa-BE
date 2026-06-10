using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Notifications.V1.MarkAllNotificationsRead;

public class MarkAllNotificationsReadCommandHandler(IRepository<UserNotification> repo)
    : IRequestHandler<MarkAllNotificationsReadCommand>
{
    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken ct)
    {
        var unread = await repo.Query()
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(ct);
        foreach (var n in unread) n.IsRead = true;
        await repo.SaveChangesAsync();
    }
}
