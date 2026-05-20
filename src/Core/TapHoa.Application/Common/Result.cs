namespace TapHoa.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(string error, string? errorCode)
    {
        IsSuccess = false;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string error, string? errorCode = null) => new(error, errorCode);

    public static implicit operator Result<T>(T value) => Ok(value);
}
