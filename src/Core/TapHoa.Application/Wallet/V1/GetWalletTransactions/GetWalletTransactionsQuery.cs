using MediatR;
using TapHoa.Application.Common;
using TapHoa.Application.Wallet.V1.GetWallet;

namespace TapHoa.Application.Wallet.V1.GetWalletTransactions;

public record GetWalletTransactionsQuery(
    Guid UserId,
    int Page     = 1,
    int PageSize = 20
) : IRequest<PagedResult<WalletTransactionDto>>;
