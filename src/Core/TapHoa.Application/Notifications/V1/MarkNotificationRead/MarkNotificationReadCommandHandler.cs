using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Notifications.V1.MarkNotificationRead;

public class MarkNotificationReadCommandHandler(IRepository<UserNotification> repo)
    : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken ct)
    {
        var notif = await repo.FindAsync(n => n.Id == request.NotificationId && n.UserId == request.UserId);
        if (notif is null) return false;
        notif.IsRead = true;
        await repo.SaveChangesAsync();
        return true;
    }
}
