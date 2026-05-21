using FluentValidation;

namespace TapHoa.Application.Agent.V1.ReportDamaged;

public class ReportDamagedGoodsCommandValidator : AbstractValidator<ReportDamagedGoodsCommand>
{
    public ReportDamagedGoodsCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.DamagedQuantity)
            .GreaterThan(0).WithMessage("Số lượng hàng lỗi phải lớn hơn 0.");
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Vui lòng nhập lý do hàng lỗi.")
            .MaximumLength(1000);
    }
}
