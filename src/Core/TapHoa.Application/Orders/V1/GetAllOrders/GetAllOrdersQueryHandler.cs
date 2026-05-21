using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.GetAllOrders;

public class GetAllOrdersQueryHandler(IRepository<Order> orderRepo)
    : IRequestHandler<GetAllOrdersQuery, Result<PagedResult<AdminOrderResponse>>>
{
    public async Task<Result<PagedResult<AdminOrderResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Order> query = orderRepo.Query()
            .Include(o => o.User)
            .Include(o => o.Hub)
            .Include(o => o.Items).ThenInclude(i => i.Product);

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var kw = request.Search.Trim().ToLower();
            query = query.Where(o =>
                o.User.FullName.ToLower().Contains(kw) ||
                o.User.Email.ToLower().Contains(kw) ||
                o.Hub.Name.ToLower().Contains(kw));
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return Result<PagedResult<AdminOrderResponse>>.Ok(new PagedResult<AdminOrderResponse>
        {
            Items = items.Select(MapToAdminResponse).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }

    private static AdminOrderResponse MapToAdminResponse(Order o) => new(
        o.Id,
        o.UserId,
        o.User.FullName,
        o.User.Email,
        o.Status,
        o.TotalAmount,
        o.Note,
        new HubInfo(o.Hub.Id, o.Hub.Name, o.Hub.Address, o.Hub.Ward, o.Hub.District, o.Hub.City, o.Hub.Latitude, o.Hub.Longitude),
        o.Items.Select(i => new OrderItemResponse(
            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.ThumbnailUrl,
            i.Quantity,
            i.UnitPrice,
            i.UnitPrice * i.Quantity
        )).ToList(),
        o.CreatedAt,
        o.CancelReason,
        o.ShippingToHubAt,
        o.InHubAt,
        o.CompletedAt,
        o.CancelledAt,
        o.RefundedAt,
        CanStartShipping: o.Status == OrderStatus.Paid_WaitingForBatch,
        CanMarkInHub:     o.Status == OrderStatus.ShippingToHub,
        CanComplete:      o.Status == OrderStatus.InHub_ReadyForPickup,
        CanCancel:        o.Status == OrderStatus.Paid_WaitingForBatch
    );
}
