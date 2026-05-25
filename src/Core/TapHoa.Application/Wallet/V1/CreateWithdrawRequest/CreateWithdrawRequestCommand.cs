using MediatR;

namespace TapHoa.Application.Wallet.V1.CreateWithdrawRequest;

public record CreateWithdrawRequestCommand(
    Guid UserId,
    decimal Amount,
    string BankName,
    string AccountNumber,
    string HolderName) : IRequest<Guid>;
