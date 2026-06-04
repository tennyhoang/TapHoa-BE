namespace TapHoa.Application.Contracts;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string fullName, string token, CancellationToken ct = default);
    Task SendPasswordResetAsync(string email, string fullName, string token, CancellationToken ct = default);
    Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default);
}
