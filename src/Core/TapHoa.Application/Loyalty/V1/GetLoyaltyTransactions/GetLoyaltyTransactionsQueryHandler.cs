using MediatR;
using TapHoa.Application.Common;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Loyalty.V1.GetLoyaltyTransactions;

public class GetLoyaltyTransactionsQueryHandler(ILoyaltyRepository loyaltyRepo)
    : IRequestHandler<GetLoyaltyTransactionsQuery, PagedResult<LoyaltyTransactionDto>>
{
    public async Task<PagedResult<LoyaltyTransactionDto>> Handle(
        GetLoyaltyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var totalCount = await loyaltyRepo.CountTransactionsAsync(request.UserId, cancellationToken);

        var items = await loyaltyRepo.GetTransactionsAsync(
            request.UserId, request.Page, request.PageSize, cancellationToken);

        return new PagedResult<LoyaltyTransactionDto>
        {
            Items = items.Select(lt => new LoyaltyTransactionDto(
                lt.Id, lt.Points, lt.Type, lt.Description, lt.OrderId, lt.CreatedAt)).ToList(),
            TotalCount = totalCount,
            Page       = request.Page,
            PageSize   = request.PageSize,
        };
    }
}
