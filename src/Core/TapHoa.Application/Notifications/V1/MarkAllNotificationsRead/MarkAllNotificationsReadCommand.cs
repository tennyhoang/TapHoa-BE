using MediatR;

namespace TapHoa.Application.Notifications.V1.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand(Guid UserId) : IRequest;
