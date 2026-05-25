using MediatR;

namespace TapHoa.Application.Wallet.V1.WithdrawWallet;

public record WithdrawWalletCommand(Guid UserId, decimal Amount) : IRequest<WithdrawWalletResult>;
public record WithdrawWalletResult(decimal NewBalance, decimal Amount);
