using FluentValidation;

namespace TapHoa.Application.HubInventories.V1.UpdateHubStock;

public class UpdateHubStockCommandValidator : AbstractValidator<UpdateHubStockCommand>
{
    public UpdateHubStockCommandValidator()
    {
        RuleFor(x => x.HubId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.NewStock)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Số lượng tồn kho không được âm.");
    }
}
