using MediatR;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Wallet.V1.GetWallet;

public record GetWalletQuery(Guid UserId) : IRequest<WalletResponse>;

public record WalletResponse(
    decimal Balance,
    IReadOnlyList<WalletTransactionDto> RecentTransactions);

public record WalletTransactionDto(
    Guid Id,
    decimal Amount,
    WalletTransactionType Type,
    string Description,
    DateTime CreatedAt);
