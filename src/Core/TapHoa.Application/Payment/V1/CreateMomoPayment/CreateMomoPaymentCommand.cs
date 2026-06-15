namespace TapHoa.Application.Payment.V1.CreateMomoPayment;

public record CreateMomoPaymentCommand(Guid OrderId, Guid UserId) : IRequest<MomoPaymentResponse>;
