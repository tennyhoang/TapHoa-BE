using MediatR;
using Microsoft.EntityFrameworkCore;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.CreateOrder;

public class CreateOrderCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<CartItem> cartRepo,
    IRepository<Hub> hubRepo,
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletTransactionRepo)
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

        var orderId      = Guid.NewGuid();
        var totalAmount  = orderItems.Sum(i => i.UnitPrice * i.Quantity);
        var isCod        = string.Equals(request.PaymentMethod, "COD",    StringComparison.OrdinalIgnoreCase);
        var isWallet     = string.Equals(request.PaymentMethod, "Wallet", StringComparison.OrdinalIgnoreCase);

        // ── Wallet deduction (full or partial) ─────────────────────────────────
        User? buyer = null;
        decimal walletAmountUsed = 0;

        if (isWallet || request.UseWallet)
        {
            buyer = await userRepo.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

            walletAmountUsed = isWallet
                ? totalAmount                                    // full wallet payment
                : Math.Min(buyer.WalletBalance, totalAmount);    // partial: use all available balance

            buyer.DebitWallet(walletAmountUsed);
            userRepo.Update(buyer);
        }

        var remainingAmount = totalAmount - walletAmountUsed;

        // PaymentRef only needed when a bank transfer is still required
        var needsBankTransfer = !isCod && !isWallet && remainingAmount > 0;
        var paymentRef = needsBankTransfer ? "TH" + orderId.ToString("N")[..8].ToUpper() : null;

        var order = new Order
        {
            Id               = orderId,
            UserId           = request.UserId,
            HubId            = request.HubId,
            TotalAmount      = totalAmount,
            WalletAmountUsed = walletAmountUsed,
            Note             = request.Note,
            Items            = orderItems,
            PaymentRef       = paymentRef,
        };

        // Confirm immediately when no pending bank transfer: COD, full wallet, hybrid+COD, or wallet covers full amount
        if (!needsBankTransfer) order.ConfirmPayment();

        await orderRepo.AddAsync(order);

        foreach (var item in cartItems)
            cartRepo.Remove(item);

        await orderRepo.SaveChangesAsync();

        if (walletAmountUsed > 0 && buyer is not null)
        {
            var shortRef = orderId.ToString()[..8].ToUpper();
            await walletTransactionRepo.AddAsync(new WalletTransaction
            {
                UserId      = buyer.Id,
                Amount      = walletAmountUsed,
                Type        = WalletTransactionType.Debit,
                Description = isWallet
                    ? $"Thanh toán đơn hàng #{shortRef}"
                    : $"Thanh toán một phần đơn hàng #{shortRef} qua ví",
                OrderId     = orderId,
            });
            await walletTransactionRepo.SaveChangesAsync();
        }

        return MapToResponse(order, hub, cartItems);
    }

    internal static OrderResponse MapToResponse(Order o, Hub hub, List<CartItem>? cartItems = null) => new(
        o.Id, o.Status, o.TotalAmount, o.WalletAmountUsed, o.Note,
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
