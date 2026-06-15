using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Payment.V1.ProcessRefund;

public record ProcessRefundCommand(Guid OrderId, string? Reason) : IRequest<Result<ProcessRefundResult>>;

public record ProcessRefundResult(string Message);