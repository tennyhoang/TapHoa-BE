namespace TapHoa.Application.Contracts;

public interface IOrderTrackingService
{
    Task NotifyOrderStatusChangedAsync(Guid userId, Guid orderId, string status, CancellationToken cancellationToken = default);
}
