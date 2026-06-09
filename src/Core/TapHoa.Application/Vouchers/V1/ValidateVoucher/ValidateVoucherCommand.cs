using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Vouchers.V1.ValidateVoucher;

public record ValidateVoucherCommand(string Code, decimal Total)
    : IRequest<Result<VoucherResponse>>;
