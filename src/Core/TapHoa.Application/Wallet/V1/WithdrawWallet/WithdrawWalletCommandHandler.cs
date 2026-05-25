using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.WithdrawWallet;

public class WithdrawWalletCommandHandler(
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletRepo)
    : IRequestHandler<WithdrawWalletCommand, WithdrawWalletResult>
{
    public async Task<WithdrawWalletResult> Handle(WithdrawWalletCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        user.DebitWallet(request.Amount);

        await walletRepo.AddAsync(new WalletTransaction
        {
            UserId      = user.Id,
            Amount      = request.Amount,
            Type        = WalletTransactionType.Debit,
            Description = $"Rút tiền từ ví",
        });

        await walletRepo.SaveChangesAsync();

        return new WithdrawWalletResult(user.WalletBalance, request.Amount);
    }
}
