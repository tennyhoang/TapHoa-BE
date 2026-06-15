using FluentResults;

namespace TapHoa.Application.Common.OneOf;

public static class OneOfResult
{
    public static FluentResults.Result<T> ToFluentResult<T>(object result)
    {
        if (result is FluentResults.Result<T> fluent)
            return fluent;

        return result switch
        {
            NotFound nf => FluentResults.Result.Fail<T>(
                new Error(nf.Message).WithMetadata("ErrorCode", nf.ErrorCode ?? "NOT_FOUND")),
            ValidationError ve => FluentResults.Result.Fail<T>(
                new Error(ve.Message).WithMetadata("ErrorCode", ve.ErrorCode ?? "VALIDATION_ERROR")),
            Conflict c => FluentResults.Result.Fail<T>(
                new Error(c.Message).WithMetadata("ErrorCode", c.ErrorCode ?? "CONFLICT")),
            Unauthorized ua => FluentResults.Result.Fail<T>(
                new Error(ua.Message).WithMetadata("ErrorCode", ua.ErrorCode ?? "UNAUTHORIZED")),
            Forbidden fb => FluentResults.Result.Fail<T>(
                new Error(fb.Message).WithMetadata("ErrorCode", fb.ErrorCode ?? "FORBIDDEN")),
            null => FluentResults.Result.Fail<T>("Null result"),
            _ => FluentResults.Result.Ok((T)result)
        };
    }
}
