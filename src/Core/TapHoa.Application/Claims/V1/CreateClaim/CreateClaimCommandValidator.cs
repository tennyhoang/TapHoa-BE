using FluentValidation;

namespace TapHoa.Application.Claims.V1.CreateClaim;

public class CreateClaimCommandValidator : AbstractValidator<CreateClaimCommand>
{
    public CreateClaimCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000)
            .WithMessage("Lý do khiếu nại không được rỗng và không quá 1000 ký tự.");
        RuleFor(x => x.ImageUrl).MaximumLength(2048).When(x => x.ImageUrl is not null);
    }
}
