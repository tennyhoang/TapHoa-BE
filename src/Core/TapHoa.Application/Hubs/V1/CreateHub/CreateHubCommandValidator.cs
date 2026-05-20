using FluentValidation;

namespace TapHoa.Application.Hubs.V1.CreateHub;

public class CreateHubCommandValidator : AbstractValidator<CreateHubCommand>
{
    public CreateHubCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên Hub không được để trống.")
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Địa chỉ không được để trống.")
            .MaximumLength(500);

        RuleFor(x => x.Ward)
            .NotEmpty().WithMessage("Phường/Xã không được để trống.")
            .MaximumLength(100);

        RuleFor(x => x.District)
            .NotEmpty().WithMessage("Quận/Huyện không được để trống.")
            .MaximumLength(100);

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Tỉnh/Thành phố không được để trống.")
            .MaximumLength(100);

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude phải từ -90 đến 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude phải từ -180 đến 180.");
    }
}
