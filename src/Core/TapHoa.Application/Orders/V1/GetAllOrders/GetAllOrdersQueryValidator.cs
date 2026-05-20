using FluentValidation;

namespace TapHoa.Application.Orders.V1.GetAllOrders;

public class GetAllOrdersQueryValidator : AbstractValidator<GetAllOrdersQuery>
{
    public GetAllOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page phải lớn hơn 0.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize phải từ 1 đến 100.");
        RuleFor(x => x.Search).MaximumLength(100).When(x => x.Search is not null);
    }
}
