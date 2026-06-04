namespace TapHoa.Worker.Messages;

public record OrderPaidMessage(Guid OrderId, Guid UserId, decimal Amount, DateTime PaidAt);
