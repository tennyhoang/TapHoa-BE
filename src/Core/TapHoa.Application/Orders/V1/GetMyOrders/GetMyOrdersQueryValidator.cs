using FluentValidation;

namespace TapHoa.Application.Orders.V1.GetMyOrders;

public class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    public GetMyOrdersQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0).WithMessage("Page phải lớn hơn 0.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("PageSize phải từ 1 đến 100.");
        RuleFor(x => x.UserId).NotEmpty();
    }
}
