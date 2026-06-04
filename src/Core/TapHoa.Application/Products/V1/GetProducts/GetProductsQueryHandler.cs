using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TapHoa.Application.Common;
using TapHoa.Application.Contracts;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Products.V1.GetProducts;

public class GetProductsQueryHandler(
    IRepository<Product> productRepo,
    IRepository<Hub> hubRepo,
    IDistributedCache cache)
    : IRequestHandler<GetProductsQuery, PagedResult<ProductResponse>>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    public async Task<PagedResult<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // ── Hub validation ─────────────────────────────────────────────────────
        if (request.HubId.HasValue)
        {
            var hubActive = await hubRepo.AnyAsync(
                h => h.Id == request.HubId.Value && h.Status == HubStatus.Active);

            if (!hubActive)
                throw new KeyNotFoundException("Hub không tồn tại hoặc đã ngừng hoạt động.");
        }

        // ── Cache ──────────────────────────────────────────────────────────────
        var cacheKey = GetCacheKey(request);
        var cached = await CacheHelper.GetAsync<PagedResult<ProductResponse>>(cache, cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        // ── Base query ─────────────────────────────────────────────────────────
        var query = productRepo.Query()
            .Include(p => p.Category).ThenInclude(c => c.Parent)
            .Include(p => p.Images)
            .Include(p => p.Reviews)
            .Where(p => p.IsActive);

        // ── Hub inventory filter ───────────────────────────────────────────────
        if (request.HubId.HasValue)
        {
            var hubId = request.HubId.Value;
            query = query.Where(p =>
                p.HubInventories.Any(hi => hi.HubId == hubId && hi.Stock > 0));
        }

        // ── Filters ────────────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Category.Name.ToLower().Contains(term) ||
                (p.Category.Parent != null && p.Category.Parent.Name.ToLower().Contains(term)));
        }

        // Match exact category OR any of its direct children (2-level hierarchy).
        if (request.CategoryId.HasValue)
            query = query.Where(p =>
                p.CategoryId == request.CategoryId ||
                p.Category.ParentId == request.CategoryId);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice);

        if (request.IsDiscount == true)
            query = query.Where(p => p.DiscountPrice != null && p.DiscountPrice > 0);

        // ── Sort ───────────────────────────────────────────────────────────────
        query = request.IsNew == true
            ? query.OrderByDescending(p => p.CreatedAt)
            : request.IsDiscount == true
                ? query.OrderBy(p => p.DiscountPrice)
                : request.SortBy switch
                {
                    "price_asc"  => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "name"       => query.OrderBy(p => p.Name),
                    _            => query.OrderByDescending(p => p.CreatedAt)
                };

        // ── Paginate ───────────────────────────────────────────────────────────
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var result = new PagedResult<ProductResponse>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };

        await CacheHelper.SetAsync(cache, cacheKey, result, CacheTtl, cancellationToken);
        return result;
    }

    internal static string GetCacheKey(GetProductsQuery query)
    {
        var key = $"{query.Search}|{query.CategoryId}|{query.MinPrice}|{query.MaxPrice}|{query.SortBy}|{query.Page}|{query.PageSize}|{query.HubId}|{query.IsNew}|{query.IsDiscount}";
        var bytes = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        return $"products:list:{Convert.ToHexString(bytes)[..16]}";
    }

    internal static ProductResponse MapToResponse(Product p) => new(
        p.Id, p.Name, p.Description, p.Price, p.DiscountPrice, p.Stock,
        p.ThumbnailUrl, p.CategoryId, p.Category.Name,
        p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
        p.Reviews.Count,
        p.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
        p.CreatedAt
    );
}
