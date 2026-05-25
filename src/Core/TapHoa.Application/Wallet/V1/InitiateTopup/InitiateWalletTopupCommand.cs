using MediatR;

namespace TapHoa.Application.Wallet.V1.InitiateTopup;

public record InitiateWalletTopupCommand(Guid UserId, decimal Amount)
    : IRequest<InitiateTopupResult>;

public record InitiateTopupResult(string PaymentRef, decimal Amount);
