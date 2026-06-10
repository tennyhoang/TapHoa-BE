using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Driver.V1.ReportDeliveryFailure;

public record ReportDeliveryFailureCommand(Guid OrderId, string? Reason)
    : IRequest<Result<DeliveryFailureResponse>>;

public record DeliveryFailureResponse(int AttemptsCount, bool AutoCancelled, string Message);
