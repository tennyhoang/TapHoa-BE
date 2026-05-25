using MediatR;

namespace TapHoa.Application.Wallet.V1.TopupWallet;

public record TopupWalletCommand(Guid UserId, decimal Amount) : IRequest<TopupWalletResult>;
public record TopupWalletResult(decimal NewBalance, decimal Amount);
