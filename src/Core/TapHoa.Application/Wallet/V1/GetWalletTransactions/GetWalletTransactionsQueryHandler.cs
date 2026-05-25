using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Application.Wallet.V1.GetWallet;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.GetWalletTransactions;

public class GetWalletTransactionsQueryHandler(IRepository<WalletTransaction> walletTransactionRepo)
    : IRequestHandler<GetWalletTransactionsQuery, PagedResult<WalletTransactionDto>>
{
    public async Task<PagedResult<WalletTransactionDto>> Handle(
        GetWalletTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var baseQuery = walletTransactionRepo.Query()
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.CreatedAt);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new WalletTransactionDto(t.Id, t.Amount, t.Type, t.Description, t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<WalletTransactionDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize,
        };
    }
}
