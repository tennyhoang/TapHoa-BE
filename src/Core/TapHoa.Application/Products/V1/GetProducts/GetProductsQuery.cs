using MediatR;
using TapHoa.Application.Common;

namespace TapHoa.Application.Products.V1.GetProducts;

public record GetProductsQuery(
    string? Search,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string SortBy = "newest",
    int Page = 1,
    int PageSize = 20,
    Guid? HubId = null,
    bool? IsNew = null,
    bool? IsDiscount = null
) : IRequest<PagedResult<ProductResponse>>;
