namespace TapHoa.Application.Contracts;

/// <summary>
/// Truy cập thông tin của User đang đăng nhập trong scope HTTP hiện tại.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Id của User đang đăng nhập (từ claim "sub").</summary>
    Guid UserId { get; }

    /// <summary>True nếu request đã được xác thực.</summary>
    bool IsAuthenticated { get; }
}
