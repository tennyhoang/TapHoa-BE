using TapHoa.Domain.Enums;

namespace TapHoa.Application.Claims.V1;

public record ClaimResponse(
    Guid Id,
    Guid OrderId,
    string CustomerName,
    string Reason,
    string? ImageUrl,
    ClaimStatus Status,
    DateTime CreatedAt
);
