using FluentValidation;
using MediatR;

namespace TapHoa.Application.Common;

public class ValidationFailure
{
    public required string PropertyName { get; init; }
    public required string ErrorMessage { get; init; }
    public string? AttemptedValue { get; init; }
}

public class RequestValidationException(IReadOnlyList<ValidationFailure> failures)
    : Exception("Validation failed.")
{
    public IReadOnlyList<ValidationFailure> Failures { get; } = failures;
}

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => new ValidationFailure
            {
                PropertyName = f.PropertyName,
                ErrorMessage = f.ErrorMessage,
                AttemptedValue = f.AttemptedValue?.ToString()
            })
            .ToList();

        if (failures.Count > 0)
            throw new RequestValidationException(failures);

        return await next(cancellationToken);
    }
}
