using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TapHoa.Application.Contracts;
using TapHoa.Application.Loyalty;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Enums;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Orders.V1.CreateOrder;

public class CreateOrderCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<CartItem> cartRepo,
    IRepository<Hub> hubRepo,
    IRepository<User> userRepo,
    IRepository<WalletTransaction> walletTransactionRepo,
    IRepository<Voucher> voucherRepo,
    IFlashSaleRepository flashSaleRepo,
    IRepository<InventoryTransaction> inventoryTxRepo,
    ILoyaltyRepository loyaltyRepo,
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    IOptions<LoyaltyOptions> options)
    : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var hub = await hubRepo.FindAsync(h => h.Id == request.HubId && h.Status == HubStatus.Active)
            ?? throw new KeyNotFoundException("Điểm nhận hàng (Hub) không tồn tại hoặc đã ngừng hoạt động.");

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

        var productIds = cartItems.Select(c => c.ProductId).ToList();

        // ── Flash Sale: load active items and pre-check stock ──────────────────
        var flashSaleItems = await flashSaleRepo.GetActiveByProductIdsAsync(productIds, cancellationToken);
        var flashSaleMap   = flashSaleItems.ToDictionary(f => f.ProductId);

        foreach (var item in cartItems)
        {
            if (flashSaleMap.TryGetValue(item.ProductId, out var fsItem)
                && fsItem.FlashSaleStock < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Sản phẩm '{item.Product.Name}' chỉ còn {fsItem.FlashSaleStock} suất Flash Sale.");
            }
        }

        // ── Deduct stock + audit trail (BR-012) ───────────────────────────────
        var inventoryLogs = cartItems.Select(item =>
        {
            var before = item.Product.Stock;
            item.Product.Stock -= item.Quantity;
            return new InventoryTransaction
            {
                ProductId      = item.ProductId,
                ActorUserId    = request.UserId,
                Type           = InventoryTransactionType.HardReserve,
                QuantityBefore = before,
                QuantityAfter  = item.Product.Stock,
                Reason         = $"Đặt hàng — đơn tạm thời",
            };
        }).ToList();

        var orderItems = cartItems.Select(c => new OrderItem
        {
            ProductId = c.ProductId,
            Quantity  = c.Quantity,
            UnitPrice = c.Product.DiscountPrice ?? c.Product.Price
        }).ToList();

        var orderId     = Guid.NewGuid();
        var totalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity);

        // ── BR-013: minimum order value check ─────────────────────────────────
        if (totalAmount < hub.MinimumOrderAmount)
            throw new InvalidOperationException(
                $"Giá trị đơn hàng tối thiểu là {hub.MinimumOrderAmount:N0}đ. " +
                $"Cần mua thêm {(hub.MinimumOrderAmount - totalAmount):N0}đ.");

        // ── Shipping fee (waived when totalAmount >= FreeShippingThreshold) ────
        var shippingFee = totalAmount >= hub.FreeShippingThreshold ? 0m : hub.ShippingFee;
        totalAmount += shippingFee;
        var isCod         = string.Equals(request.PaymentMethod, "COD",    StringComparison.OrdinalIgnoreCase);
        var isWallet      = string.Equals(request.PaymentMethod, "Wallet", StringComparison.OrdinalIgnoreCase);
        var isVnpayOrMomo = string.Equals(request.PaymentMethod, "Vnpay", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(request.PaymentMethod, "Momo",  StringComparison.OrdinalIgnoreCase);

        // ── BR-014: Voucher discount ──────────────────────────────────────────
        decimal voucherDiscount = 0;
        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            var voucher = await voucherRepo.Query()
                .FirstOrDefaultAsync(v => v.Code == request.VoucherCode.ToUpper() && v.IsActive, cancellationToken);

            if (voucher is not null
                && (!voucher.ExpiresAt.HasValue || voucher.ExpiresAt > DateTime.UtcNow)
                && (!voucher.UsageLimit.HasValue || voucher.UsedCount < voucher.UsageLimit)
                && (!voucher.MinOrderAmount.HasValue || totalAmount >= voucher.MinOrderAmount))
            {
                var applicableTotal = totalAmount - shippingFee;
                if (voucher.ExcludeFlashSaleItems)
                {
                    var flashSaleProductIds = flashSaleMap.Keys.ToHashSet();
                    applicableTotal = orderItems
                        .Where(i => !flashSaleProductIds.Contains(i.ProductId))
                        .Sum(i => i.UnitPrice * i.Quantity);
                }

                voucherDiscount = voucher.Type == "percent"
                    ? applicableTotal * voucher.DiscountValue / 100
                    : voucher.DiscountValue;

                if (voucher.MaxDiscountAmount.HasValue)
                    voucherDiscount = Math.Min(voucherDiscount, voucher.MaxDiscountAmount.Value);

                voucherDiscount = Math.Min(voucherDiscount, totalAmount);
                voucher.UsedCount++;
            }
            totalAmount -= voucherDiscount;
        }

        // ── Points redemption (1 point = 200 VND) ──────────────────────────────
        int pointsRedeemed = 0;
        decimal pointsDiscount = 0;
        if (request.PointsToRedeem > 0)
        {
            var loyaltyAccount = await loyaltyRepo.GetAccountAsync(request.UserId, cancellationToken);
            if (loyaltyAccount is null)
                throw new InvalidOperationException("Tài khoản điểm tích lũy không tồn tại.");

            var maxDiscount = totalAmount;
            var rawDiscount = request.PointsToRedeem * options.Value.RedeemValuePerPoint;
            pointsDiscount = Math.Min(rawDiscount, maxDiscount);
            pointsRedeemed = (int)Math.Ceiling(pointsDiscount / options.Value.RedeemValuePerPoint);

            if (loyaltyAccount.PointsBalance < pointsRedeemed)
                throw new InvalidOperationException("Không đủ điểm tích lũy để đổi.");

            totalAmount -= pointsDiscount;
        }

        User? buyer = null;
        decimal walletAmountUsed = 0;

        if (isWallet || request.UseWallet)
        {
            buyer = await userRepo.GetByIdAsync(request.UserId)
                ?? throw new KeyNotFoundException("Người dùng không tồn tại.");

            walletAmountUsed = isWallet
                ? totalAmount
                : Math.Min(buyer.WalletBalance, totalAmount);

            buyer.DebitWallet(walletAmountUsed);
            userRepo.Update(buyer);
        }

        var remainingAmount = totalAmount - walletAmountUsed;
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
            PaymentMethod    = request.PaymentMethod,
        };

        if (isVnpayOrMomo)
            order.MarkAwaitingPayment();
        else if (!needsBankTransfer)
            order.ConfirmPayment();

        // ── Transaction: atomically decrement flash sale stock + persist order ──
        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // FOR UPDATE via UPDATE ... WHERE FlashSaleStock >= qty (atomic, no oversell)
            foreach (var item in cartItems.Where(i => flashSaleMap.ContainsKey(i.ProductId)))
            {
                var fsItem = flashSaleMap[item.ProductId];
                var ok = await flashSaleRepo.TryDecrementStockAsync(fsItem.Id, item.Quantity, cancellationToken);
                if (!ok)
                    throw new InvalidOperationException(
                        $"Sản phẩm '{item.Product.Name}' đã hết suất Flash Sale. Vui lòng thử lại.");
            }

            await orderRepo.AddAsync(order);
            foreach (var item in cartItems)
                cartRepo.Remove(item);
            await orderRepo.SaveChangesAsync();

            foreach (var log in inventoryLogs)
                log.OrderId = order.Id;
            foreach (var log in inventoryLogs)
                await inventoryTxRepo.AddAsync(log);
            await inventoryTxRepo.SaveChangesAsync();

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

            if (pointsRedeemed > 0)
            {
                await loyaltyRepo.RedeemAsync(request.UserId, pointsRedeemed, orderId, cancellationToken);
            }

            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        // ── Confirmation email (fire-and-forget, non-blocking) ─────────────────
        _ = SendConfirmationEmailAsync(request.UserId, order, hub, cartItems, shippingFee, voucherDiscount);

        return MapToResponse(order, hub, cartItems, shippingFee, voucherDiscount, pointsRedeemed, pointsDiscount);
    }

    private async Task SendConfirmationEmailAsync(
        Guid userId, Order order, Hub hub, List<CartItem> cartItems,
        decimal shippingFee, decimal voucherDiscount)
    {
        try
        {
            var user = await userRepo.GetByIdAsync(userId);
            if (user is null) return;

            var shortRef   = order.Id.ToString()[..8].ToUpper();
            var itemsSummary = string.Join(", ", cartItems.Select(c => $"{c.Product.Name} x{c.Quantity}"));
            var body = $"""
                <h2>Xác nhận đơn hàng #{shortRef}</h2>
                <p>Chào {user.FullName},</p>
                <p>Đơn hàng của bạn đã được đặt thành công!</p>
                <table style="width:100%;border-collapse:collapse">
                  <tr><td><strong>Mã đơn:</strong></td><td>#{shortRef}</td></tr>
                  <tr><td><strong>Điểm lấy hàng:</strong></td><td>{hub.Name} — {hub.Address}, {hub.Ward}, {hub.District}</td></tr>
                  <tr><td><strong>Sản phẩm:</strong></td><td>{itemsSummary}</td></tr>
                  {(shippingFee > 0 ? $"<tr><td><strong>Phí giao:</strong></td><td>{shippingFee:N0}đ</td></tr>" : "")}
                  {(voucherDiscount > 0 ? $"<tr><td><strong>Giảm giá:</strong></td><td>-{voucherDiscount:N0}đ</td></tr>" : "")}
                  <tr><td><strong>Tổng tiền:</strong></td><td><strong>{order.TotalAmount:N0}đ</strong></td></tr>
                  {(order.PaymentRef is not null ? $"<tr><td><strong>Mã chuyển khoản:</strong></td><td>{order.PaymentRef}</td></tr>" : "")}
                </table>
                <p style="margin-top:16px">Cảm ơn bạn đã tin tưởng TapHoa!</p>
                """;

            await emailService.SendEmailAsync(user.Email, $"Xác nhận đơn hàng #{shortRef} — TapHoa", body);
        }
        catch
        {
            // Email failure must not break the order flow
        }
    }

    internal static OrderResponse MapToResponse(Order o, Hub hub, List<CartItem>? cartItems = null,
        decimal shippingFee = 0m, decimal voucherDiscount = 0m,
        int pointsRedeemed = 0, decimal pointsDiscount = 0m) => new(
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
        o.RefundedAt,
        shippingFee,
        voucherDiscount,
        pointsRedeemed,
        pointsDiscount
    );
}
