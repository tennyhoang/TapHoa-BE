namespace TapHoa.Application.Agent.V1.ReportDamaged;

public record DamagedReportResponse(
    Guid Id,
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    int DamagedQuantity,
    decimal UnitPrice,
    decimal RefundAmount,
    string Reason,
    string Status,
    DateTime CreatedAt
);
