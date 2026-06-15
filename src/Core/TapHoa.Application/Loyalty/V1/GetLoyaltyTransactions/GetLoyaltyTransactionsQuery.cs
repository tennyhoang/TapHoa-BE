using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Loyalty.V1.GetLoyaltyTransactions;

public record GetLoyaltyTransactionsQuery(
    Guid UserId,
    int Page     = 1,
    int PageSize = 20
) : IRequest<PagedResult<LoyaltyTransactionDto>>;

public record LoyaltyTransactionDto(
    Guid Id,
    int Points,
    string Type,
    string Description,
    Guid? OrderId,
    DateTime CreatedAt
);
