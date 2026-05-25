using MediatR;

namespace TapHoa.Application.Orders.V1.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    Guid HubId,
    string? Note,
    string PaymentMethod = "BankTransfer"
) : IRequest<OrderResponse>;
