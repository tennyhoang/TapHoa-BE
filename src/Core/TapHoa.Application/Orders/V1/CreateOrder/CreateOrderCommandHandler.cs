using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.CreateOrder;

public class CreateOrderCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<CartItem> cartRepo,
    IRepository<Hub> hubRepo)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // ── Validate Hub ───────────────────────────────────────────────────────
        var hub = await hubRepo.FindAsync(h => h.Id == request.HubId && h.Status == HubStatus.Active)
            ?? throw new KeyNotFoundException("Điểm nhận hàng (Hub) không tồn tại hoặc đã ngừng hoạt động.");

        // ── Validate cart ──────────────────────────────────────────────────────
        var cartItems = await cartRepo.Query()
            .Include(c => c.Product)
            .Where(c => c.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0)
            throw new InvalidOperationException("Giỏ hàng đang trống.");

        foreach (var item in cartItems)
        {
            if (item.Product is null)
                throw new InvalidOperationException("Một sản phẩm trong giỏ hàng không còn tồn tại.");

            if (!item.Product.IsActive)
                throw new InvalidOperationException($"Sản phẩm '{item.Product.Name}' không còn được bán.");

            if (item.Product.Stock < item.Quantity)
                throw new InvalidOperationException(
                    $"Sản phẩm '{item.Product.Name}' chỉ còn {item.Product.Stock} trong kho.");
        }

        // ── Deduct stock (tracked entities — no Update() needed) ──────────────
        foreach (var item in cartItems)
            item.Product.Stock -= item.Quantity;

        // ── Build order ────────────────────────────────────────────────────────
        var orderItems = cartItems.Select(c => new OrderItem
        {
            ProductId = c.ProductId,
            Quantity  = c.Quantity,
            UnitPrice = c.Product.DiscountPrice ?? c.Product.Price
        }).ToList();

        var orderId = Guid.NewGuid();
        var paymentRef = "TH" + orderId.ToString("N")[..8].ToUpper();

        var order = new Order
        {
            Id          = orderId,
            UserId      = request.UserId,
            HubId       = request.HubId,
            TotalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity),
            Note        = request.Note,
            Items       = orderItems,
            PaymentRef  = paymentRef,
        };

        await orderRepo.AddAsync(order);

        foreach (var item in cartItems)
            cartRepo.Remove(item);

        await orderRepo.SaveChangesAsync();

        return MapToResponse(order, hub, cartItems);
    }

    // Hub thay thế Address trong mô hình O2O:
    //   ReceiverName = tên Hub (điểm khách đến lấy)
    //   FullAddress  = địa chỉ đầy đủ của Hub
    // cartItems: truyền khi Create để lấy tên SP mà không cần round-trip thêm.
    // Các caller khác (Cancel, GetById, UpdateStatus) dùng ThenInclude nên không cần.
    internal static OrderResponse MapToResponse(Order o, Hub hub, List<CartItem>? cartItems = null) => new(
        o.Id, o.Status, o.TotalAmount, o.Note,
        new HubInfo(hub.Id, hub.Name, hub.Address, hub.Ward, hub.District, hub.City, hub.Latitude, hub.Longitude),
        o.Items.Select(i =>
        {
            var cart = cartItems?.FirstOrDefault(c => c.ProductId == i.ProductId);
            return new OrderItemResponse(
                i.ProductId,
                cart?.Product?.Name ?? i.Product?.Name ?? string.Empty,
                cart?.Product?.ThumbnailUrl ?? i.Product?.ThumbnailUrl,
                i.Quantity,
                i.UnitPrice,
                i.UnitPrice * i.Quantity
            );
        }).ToList(),
        o.CreatedAt,
        o.CancelReason,
        o.PaymentRef,
        o.PaidAt,
        o.ShippingToHubAt,
        o.InHubAt,
        o.CompletedAt,
        o.CancelledAt,
        o.RefundedAt
    );
}
