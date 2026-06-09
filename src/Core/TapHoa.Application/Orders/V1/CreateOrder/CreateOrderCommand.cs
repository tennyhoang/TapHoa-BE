using MediatR;

namespace TapHoa.Application.Orders.V1.CreateOrder;

public record CreateOrderCommand(
    Guid UserId,
    Guid HubId,
    string? Note,
    string PaymentMethod = "BankTransfer",
    bool UseWallet = false,
    string? VoucherCode = null
) : IRequest<OrderResponse>;
