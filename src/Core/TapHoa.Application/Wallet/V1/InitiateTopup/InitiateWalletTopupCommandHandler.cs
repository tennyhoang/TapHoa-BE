using MediatR;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Wallet.V1.InitiateTopup;

public class InitiateWalletTopupCommandHandler(IRepository<WalletTopupRequest> topupRepo)
    : IRequestHandler<InitiateWalletTopupCommand, InitiateTopupResult>
{
    public async Task<InitiateTopupResult> Handle(
        InitiateWalletTopupCommand request, CancellationToken cancellationToken)
    {
        var paymentRef = "THWL" + Guid.NewGuid().ToString("N")[..7].ToUpper();

        await topupRepo.AddAsync(new WalletTopupRequest
        {
            UserId     = request.UserId,
            Amount     = request.Amount,
            PaymentRef = paymentRef,
        });
        await topupRepo.SaveChangesAsync();

        return new InitiateTopupResult(paymentRef, request.Amount);
    }
}
