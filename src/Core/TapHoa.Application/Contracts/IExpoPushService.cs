namespace TapHoa.Application.Contracts;

public interface IExpoPushService
{
    Task SendAsync(Guid userId, string title, string body, object? data = null, CancellationToken cancellationToken = default);
}
