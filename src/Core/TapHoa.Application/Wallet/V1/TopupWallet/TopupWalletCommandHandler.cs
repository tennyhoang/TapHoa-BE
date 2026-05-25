using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.TopupWallet;

public class TopupWalletCommandHandler(
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletRepo)
    : IRequestHandler<TopupWalletCommand, TopupWalletResult>
{
    public async Task<TopupWalletResult> Handle(TopupWalletCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        user.CreditWallet(request.Amount);

        await walletRepo.AddAsync(new WalletTransaction
        {
            UserId      = user.Id,
            Amount      = request.Amount,
            Type        = WalletTransactionType.Credit,
            Description = $"Nạp tiền vào ví",
        });

        await walletRepo.SaveChangesAsync();

        return new TopupWalletResult(user.WalletBalance, request.Amount);
    }
}
