using MediatR;

namespace TapHoa.Application.Notifications.V1.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest<bool>;
