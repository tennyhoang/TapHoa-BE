using FluentValidation;

namespace TapHoa.Application.Orders.V1.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.HubId).NotEmpty().WithMessage("Vui lòng chọn điểm nhận hàng (Hub).");
        RuleFor(x => x.Note).MaximumLength(500).When(x => x.Note is not null);
    }
}
