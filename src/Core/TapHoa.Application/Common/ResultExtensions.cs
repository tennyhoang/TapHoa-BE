using FluentResults;

namespace TapHoa.Application.Common;

public static class ResultExtensions
{
    public static FluentResults.Result<T> WithErrorCode<T>(
        this FluentResults.Result<T> result, string errorCode)
    {
        if (!result.IsSuccess && result.Errors.FirstOrDefault() is { } error)
            error.Metadata["ErrorCode"] = errorCode;
        return result;
    }

    public static string? GetErrorCode<T>(this FluentResults.Result<T> result)
    {
        if (result.Errors.FirstOrDefault()?.Metadata.TryGetValue("ErrorCode", out var code) == true)
            return code as string;
        return null;
    }
}
