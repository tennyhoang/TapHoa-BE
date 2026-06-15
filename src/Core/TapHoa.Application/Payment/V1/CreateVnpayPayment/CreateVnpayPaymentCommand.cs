namespace TapHoa.Application.Payment.V1.CreateVnpayPayment;

public record CreateVnpayPaymentCommand(Guid OrderId, Guid UserId) : IRequest<VnpayPaymentResponse>;

public record VnpayPaymentResponse(string PaymentUrl, string OrderRef);
