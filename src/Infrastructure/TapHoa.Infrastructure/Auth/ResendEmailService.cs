using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using TapHoa.Application.Contracts;

namespace TapHoa.Infrastructure.Auth;

public class ResendEmailService(
    IResend resend,
    IConfiguration configuration,
    ILogger<ResendEmailService> logger)
    : IEmailService
{
    private string From => configuration["Email:From"] ?? "TapHoa <noreply@taphoa.vn>";
    private string BaseUrl => configuration["BaseUrl"] ?? "http://localhost:3000";

    public Task SendEmailConfirmationAsync(string email, string fullName, string token, CancellationToken ct = default)
    {
        var link    = $"{BaseUrl}/auth/confirm-email?token={token}";
        var subject = "Xác nhận email đăng ký TapHoa";
        var body    = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:0 auto">
              <h2 style="color:#0d9488">Chào {fullName},</h2>
              <p>Cảm ơn bạn đã đăng ký tài khoản <strong>TapHoa</strong>.</p>
              <p>Nhấn nút bên dưới để xác nhận địa chỉ email:</p>
              <a href="{link}" style="display:inline-block;padding:12px 28px;background:#0d9488;color:#fff;text-decoration:none;border-radius:8px;font-weight:bold">Xác nhận email</a>
              <p style="margin-top:24px;color:#6b7280;font-size:13px">Link có hiệu lực trong 7 ngày. Nếu bạn không đăng ký, hãy bỏ qua email này.</p>
            </div>
            """;
        return SendEmailAsync(email, subject, body, ct);
    }

    public Task SendPasswordResetAsync(string email, string fullName, string token, CancellationToken ct = default)
    {
        var link    = $"{BaseUrl}/auth/reset-password?token={token}";
        var subject = "Đặt lại mật khẩu TapHoa";
        var body    = $"""
            <div style="font-family:sans-serif;max-width:520px;margin:0 auto">
              <h2 style="color:#0d9488">Chào {fullName},</h2>
              <p>Bạn đã yêu cầu đặt lại mật khẩu tài khoản <strong>TapHoa</strong>.</p>
              <a href="{link}" style="display:inline-block;padding:12px 28px;background:#0d9488;color:#fff;text-decoration:none;border-radius:8px;font-weight:bold">Tạo mật khẩu mới</a>
              <p style="margin-top:24px;color:#6b7280;font-size:13px">Link có hiệu lực trong 1 giờ. Nếu không phải bạn, hãy bỏ qua email này.</p>
            </div>
            """;
        return SendEmailAsync(email, subject, body, ct);
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From    = From,
                Subject = subject,
                HtmlBody = body,
            };
            message.To.Add(to);
            await resend.EmailSendAsync(message, ct);
            logger.LogInformation("Email sent via Resend to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Resend failed for {To}: {Subject}", to, subject);
        }
    }
}
