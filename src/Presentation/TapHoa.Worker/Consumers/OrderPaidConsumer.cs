using MassTransit;
using TapHoa.Worker.Messages;

namespace TapHoa.Worker.Consumers;

public class OrderPaidConsumer(ILogger<OrderPaidConsumer> logger) : IConsumer<OrderPaidMessage>
{
    public Task Consume(ConsumeContext<OrderPaidMessage> context)
    {
        var msg = context.Message;
        logger.LogInformation(
            "[OrderPaid] OrderId={OrderId} UserId={UserId} Amount={Amount} PaidAt={PaidAt}",
            msg.OrderId, msg.UserId, msg.Amount, msg.PaidAt);

        // TODO: gửi email/push notification xác nhận thanh toán
        return Task.CompletedTask;
    }
}
