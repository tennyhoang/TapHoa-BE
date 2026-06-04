using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TapHoa.Application.Contracts;

namespace TapHoa.Infrastructure.Auth;

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger)
    : IEmailService
{
    private string BaseUrl => configuration["BaseUrl"] ?? "http://localhost:3000";

    public async Task SendEmailConfirmationAsync(string email, string fullName, string token, CancellationToken ct = default)
    {
        var link = $"{BaseUrl}/auth/confirm-email?token={token}";
        var subject = "Xác nhận email đăng ký TapHoa";
        var body = $"""
            <h2>Chào {fullName},</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản TapHoa.</p>
            <p>Vui lòng nhấp vào link dưới đây để xác nhận email của bạn:</p>
            <p><a href="{link}">{link}</a></p>
            <p>Link có hiệu lực trong 7 ngày.</p>
            <p>Nếu bạn không đăng ký tài khoản này, vui lòng bỏ qua email này.</p>
            """;

        await SendEmailAsync(email, subject, body, ct);
    }

    public async Task SendPasswordResetAsync(string email, string fullName, string token, CancellationToken ct = default)
    {
        var link = $"{BaseUrl}/auth/reset-password?token={token}";
        var subject = "Đặt lại mật khẩu TapHoa";
        var body = $"""
            <h2>Chào {fullName},</h2>
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản TapHoa.</p>
            <p>Vui lòng nhấp vào link dưới đây để tạo mật khẩu mới:</p>
            <p><a href="{link}">{link}</a></p>
            <p>Link có hiệu lực trong 1 giờ.</p>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            """;

        await SendEmailAsync(email, subject, body, ct);
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        var smtpHost = configuration["Email:Smtp:Host"];
        if (string.IsNullOrEmpty(smtpHost))
        {
            logger.LogInformation("[EMAIL] To: {To} | Subject: {Subject} | Body: {Body}", to, subject, body);
            return;
        }

        var smtpPort = int.Parse(configuration["Email:Smtp:Port"] ?? "587");
        var smtpUser = configuration["Email:Smtp:Username"] ?? "";
        var smtpPass = configuration["Email:Smtp:Password"] ?? "";
        var fromEmail = configuration["Email:From"] ?? "noreply@taphoa.vn";
        var fromName = configuration["Email:FromName"] ?? "TapHoa";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = string.IsNullOrEmpty(smtpUser)
                ? null
                : new NetworkCredential(smtpUser, smtpPass),
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message, ct);
            logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send email to {To}", to);
        }
    }
}
