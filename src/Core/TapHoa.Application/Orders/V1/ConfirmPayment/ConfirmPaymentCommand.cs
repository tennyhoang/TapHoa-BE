using MediatR;

namespace TapHoa.Application.Orders.V1.ConfirmPayment;

public record ConfirmPaymentCommand(SepayWebhookPayload Payload) : IRequest<bool>;
