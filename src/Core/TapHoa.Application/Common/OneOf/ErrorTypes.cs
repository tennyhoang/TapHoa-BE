namespace TapHoa.Application.Common.OneOf;

public sealed record NotFound(string Message, string? ErrorCode = null);
public sealed record ValidationError(string Message, string? ErrorCode = null);
public sealed record Conflict(string Message, string? ErrorCode = null);
public sealed record Unauthorized(string Message, string? ErrorCode = null);
public sealed record Forbidden(string Message, string? ErrorCode = null);
