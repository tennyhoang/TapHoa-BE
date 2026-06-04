using FluentResults;

namespace TapHoa.Application.Common;

// Migration bridge: converts between the legacy Result<T> and FluentResults.Result<T>.
// New code should use FluentResults directly; legacy code can migrate gradually.
public static class ResultExtensions
{
    public static FluentResults.Result<T> ToFluentResult<T>(this Result<T> result)
        => result.IsSuccess
            ? FluentResults.Result.Ok(result.Value!)
            : FluentResults.Result.Fail<T>(result.Error ?? "Unknown error");

    public static Result<T> ToLegacyResult<T>(this FluentResults.Result<T> result)
        => result.IsSuccess
            ? Result<T>.Ok(result.Value!)
            : Result<T>.Fail(string.Join("; ", result.Errors.Select(e => e.Message)));
}
