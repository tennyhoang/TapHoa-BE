namespace TapHoa.Application.Notifications.V1;

public record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Body,
    bool IsRead,
    DateTime CreatedAt,
    string? Data
);

public record PagedNotificationsResponse(
    List<NotificationResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
