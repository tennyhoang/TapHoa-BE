namespace TapHoa.Application.Contracts;

public interface IOrderStatusBroadcaster
{
    Task BroadcastStatusChanged(Guid orderId, string status, Guid userId, CancellationToken ct = default);
}
