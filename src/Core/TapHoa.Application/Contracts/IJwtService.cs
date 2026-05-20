using TapHoa.Domain.Entities;

namespace TapHoa.Application.Contracts;

public interface IJwtService
{
    string GenerateToken(User user);
}
