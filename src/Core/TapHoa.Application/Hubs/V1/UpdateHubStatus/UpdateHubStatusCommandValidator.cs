using FluentValidation;

namespace TapHoa.Application.Hubs.V1.UpdateHubStatus;

public class UpdateHubStatusCommandValidator : AbstractValidator<UpdateHubStatusCommand>
{
    public UpdateHubStatusCommandValidator()
    {
        RuleFor(x => x.HubId).NotEmpty().WithMessage("HubId không hợp lệ.");
        RuleFor(x => x.Status).IsInEnum().WithMessage("Trạng thái không hợp lệ.");
    }
}
