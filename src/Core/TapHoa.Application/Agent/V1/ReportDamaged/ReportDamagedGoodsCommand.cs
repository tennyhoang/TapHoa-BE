using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Agent.V1.ReportDamaged;

public record ReportDamagedGoodsCommand(
    Guid AgentId,
    Guid AgentHubId,
    Guid OrderId,
    Guid ProductId,
    int DamagedQuantity,
    string Reason
) : IRequest<Result<DamagedReportResponse>>;
