using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.GetWallet;

public class GetWalletQueryHandler(
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletTransactionRepo)
    : IRequestHandler<GetWalletQuery, WalletResponse>
{
    public async Task<WalletResponse> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Không tìm thấy người dùng.");

        var recentTransactions = await walletTransactionRepo.Query()
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new WalletTransactionDto(t.Id, t.Amount, t.Type, t.Description, t.CreatedAt))
            .ToListAsync(cancellationToken);

        return new WalletResponse(user.WalletBalance, recentTransactions);
    }
}
