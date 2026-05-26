using FluentValidation;

namespace TapHoa.Application.Users.V1.Admin.AssignWarehouse;

public class AssignWarehouseCommandValidator : AbstractValidator<AssignWarehouseCommand>
{
    private static readonly HashSet<string> AllowedTargetTypes =
        new(StringComparer.OrdinalIgnoreCase) { "Driver", "Manager" };

    public AssignWarehouseCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId không được để trống.");

        RuleFor(x => x.TargetType)
            .NotEmpty()
            .WithMessage("TargetType không được để trống.")
            .Must(t => AllowedTargetTypes.Contains(t))
            .WithMessage("TargetType chỉ được nhận giá trị 'Driver' hoặc 'Manager'.");
    }
}
