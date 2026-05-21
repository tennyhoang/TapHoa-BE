using FluentValidation;
using TapHoa.Domain.Enums;

namespace TapHoa.Application.Orders.V1.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.Status)
            .IsInEnum()
            .NotEqual(OrderStatus.Paid_WaitingForBatch).WithMessage("Không thể chuyển về trạng thái Paid_WaitingForBatch.");
        RuleFor(x => x.CancelReason)
            .MaximumLength(500);
    }
}
