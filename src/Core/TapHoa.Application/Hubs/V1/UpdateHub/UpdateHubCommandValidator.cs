using FluentValidation;

namespace TapHoa.Application.Hubs.V1.UpdateHub;

public class UpdateHubCommandValidator : AbstractValidator<UpdateHubCommand>
{
    public UpdateHubCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Ward).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).WithMessage("Latitude phải từ -90 đến 90.");
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).WithMessage("Longitude phải từ -180 đến 180.");
    }
}
