using Microsoft.EntityFrameworkCore;
using TapHoa.Application.Common;
using TapHoa.Domain.Entities;
using TapHoa.Domain.Repositories;

namespace TapHoa.Application.Vouchers.V1.ValidateVoucher;

public class ValidateVoucherCommandHandler(IRepository<Voucher> voucherRepo)
    : IRequestHandler<ValidateVoucherCommand, Result<VoucherResponse>>
{
    public async Task<Result<VoucherResponse>> Handle(ValidateVoucherCommand request, CancellationToken ct)
    {
        var voucher = await voucherRepo.Query()
            .FirstOrDefaultAsync(v => v.Code == request.Code.ToUpper() && v.IsActive, ct);

        if (voucher is null)
            return Result<VoucherResponse>.Fail("Mã voucher không tồn tại hoặc đã bị vô hiệu hóa.", "VOUCHER_NOT_FOUND");

        if (voucher.ExpiresAt.HasValue && voucher.ExpiresAt < DateTime.UtcNow)
            return Result<VoucherResponse>.Fail("Mã voucher đã hết hạn.", "VOUCHER_EXPIRED");

        if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit)
            return Result<VoucherResponse>.Fail("Mã voucher đã hết lượt sử dụng.", "VOUCHER_EXHAUSTED");

        if (voucher.MinOrderAmount.HasValue && request.Total < voucher.MinOrderAmount)
            return Result<VoucherResponse>.Fail(
                $"Giá trị đơn hàng tối thiểu {voucher.MinOrderAmount:N0}đ", "MIN_ORDER_NOT_MET");

        var discount = voucher.Type == "percent"
            ? request.Total * voucher.DiscountValue / 100
            : voucher.DiscountValue;

        discount = Math.Min(discount, request.Total);

        var label = voucher.Type == "percent"
            ? $"Giảm {voucher.DiscountValue:N0}% (tối đa {discount:N0}đ)"
            : $"Giảm {voucher.DiscountValue:N0}đ";

        return Result<VoucherResponse>.Ok(new VoucherResponse(discount, voucher.Type, label));
    }
}
