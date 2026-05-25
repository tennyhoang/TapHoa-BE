using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.CreateWithdrawRequest;

public class CreateWithdrawRequestCommandHandler(
    IRepository<User> userRepo,
    IRepository<WithdrawRequest> withdrawRepo,
    IRepository<WalletTransaction> walletTransactionRepo)
    : IRequestHandler<CreateWithdrawRequestCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateWithdrawRequestCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepo.GetByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

        user.DebitWallet(request.Amount); // throws OrderDomainException if insufficient

        var withdrawRequest = new WithdrawRequest
        {
            UserId        = request.UserId,
            Amount        = request.Amount,
            BankName      = request.BankName,
            AccountNumber = request.AccountNumber,
            HolderName    = request.HolderName,
        };

        await withdrawRepo.AddAsync(withdrawRequest);
        await withdrawRepo.SaveChangesAsync(); // saves user balance + request atomically

        await walletTransactionRepo.AddAsync(new WalletTransaction
        {
            UserId      = request.UserId,
            Amount      = request.Amount,
            Type        = WalletTransactionType.Debit,
            Description = $"Yêu cầu rút tiền - {request.BankName} {request.AccountNumber}",
        });
        await walletTransactionRepo.SaveChangesAsync();

        return withdrawRequest.Id;
    }
}
