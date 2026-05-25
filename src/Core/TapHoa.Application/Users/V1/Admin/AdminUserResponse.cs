namespace TapHoa.Application.Users.V1.Admin;

public record AdminUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Role,
    bool IsActive,
    Guid? AgentHubId,
    DateTime CreatedAt
);
