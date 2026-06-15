using FluentResults;

namespace TapHoa.Application.Common;

[Obsolete("Use FluentResults.Result<T> directly instead. See ResultExtensions for migration helpers.")]
public class Result<T>
{
    private readonly FluentResults.Result<T> _inner;

    public bool IsSuccess => _inner.IsSuccess;
    public T? Value => _inner.ValueOrDefault;
    public string? Error => _inner.IsSuccess ? null : string.Join("; ", _inner.Errors.Select(e => e.Message));
    public string? ErrorCode
    {
        get
        {
            if (_inner.Errors.FirstOrDefault()?.Metadata.TryGetValue("ErrorCode", out var code) == true)
                return code as string;
            return null;
        }
    }

    internal Result(FluentResults.Result<T> inner) => _inner = inner;

    public static Result<T> Ok(T value) => new(FluentResults.Result.Ok(value));
    public static Result<T> Fail(string error, string? errorCode = null)
    {
        var errorObj = new Error(error);
        if (errorCode is not null)
            errorObj.WithMetadata("ErrorCode", errorCode);
        return new Result<T>(FluentResults.Result.Fail<T>(errorObj));
    }

    public FluentResults.Result<T> ToFluent() => _inner;

    public static implicit operator Result<T>(T value) => Ok(value);
    public static implicit operator FluentResults.Result<T>(Result<T> result) => result._inner;
}
