using System.Net;
using System.Net.Mail;

namespace SnackFlowMES.Services;

/// <summary>
/// Email service for sending emails via Gmail SMTP
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
    Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string username);
    Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _log;

    public EmailService(IConfiguration config, ILogger<EmailService> log)
    {
        _config = config;
        _log = log;
    }

    /// <summary>
    /// Send email via Gmail SMTP
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            // Get SMTP settings from configuration
            var smtpHost = _config["EmailSettings:SmtpHost"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"] ?? "587");
            var smtpUsername = _config["EmailSettings:SmtpUsername"];
            var smtpPassword = _config["EmailSettings:SmtpPassword"];
            var fromEmail = _config["EmailSettings:FromEmail"];
            var fromName = _config["EmailSettings:FromName"];

            // Validate settings
            if (string.IsNullOrEmpty(smtpHost) || 
                string.IsNullOrEmpty(smtpUsername) || 
                string.IsNullOrEmpty(smtpPassword) ||
                string.IsNullOrEmpty(fromEmail))
            {
                _log.LogError("Email settings are not configured properly");
                return false;
            }

            // Create mail message
            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName ?? "SnackFlow MES"),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            message.To.Add(new MailAddress(toEmail));

            // Create SMTP client
            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                Timeout = 30000 // 30 seconds
            };

            // Send email
            await smtpClient.SendMailAsync(message);

            _log.LogInformation("Email sent successfully to {Email}", toEmail);
            return true;
        }
        catch (SmtpException ex)
        {
            _log.LogError(ex, "SMTP error sending email to {Email}: {Message}", toEmail, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Error sending email to {Email}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Send welcome email to new user
    /// </summary>
    public async Task<bool> SendWelcomeEmailAsync(string toEmail, string fullName, string username)
    {
        var subject = "Welcome to SnackFlow MES!";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #0F1C26 0%, #1a2f3d 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .logo {{ font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
        .logo-accent {{ color: #F6C000; }}
        .content {{ background: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #E87722; color: white; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: 600; }}
        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 14px; }}
        .credentials {{ background: #f3f4f6; padding: 15px; border-radius: 6px; margin: 20px 0; }}
        .credentials strong {{ color: #0F1C26; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Snack<span class='logo-accent'>Flow</span> MES</div>
            <p style='margin: 0; font-size: 16px;'>Manufacturing Execution System</p>
        </div>
        <div class='content'>
            <h2 style='color: #0F1C26; margin-top: 0;'>Welcome, {fullName}! 🎉</h2>
            <p>Your admin account has been successfully created. You now have full access to the SnackFlow MES system.</p>
            
            <div class='credentials'>
                <p style='margin: 5px 0;'><strong>Username:</strong> {username}</p>
                <p style='margin: 5px 0;'><strong>Email:</strong> {toEmail}</p>
                <p style='margin: 5px 0;'><strong>Role:</strong> Administrator</p>
            </div>

            <p><strong>What you can do as an Admin:</strong></p>
            <ul>
                <li>Manage work orders and production planning</li>
                <li>Control inventory and track materials</li>
                <li>Create and manage user accounts</li>
                <li>View comprehensive reports and analytics</li>
                <li>Configure system settings</li>
            </ul>

            <div style='text-align: center;'>
                <a href='http://localhost:5000/Dashboard' class='button'>Go to Dashboard</a>
            </div>

            <p style='margin-top: 30px; color: #6b7280; font-size: 14px;'>
                <strong>Security Tip:</strong> Keep your password secure and never share it with anyone.
            </p>
        </div>
        <div class='footer'>
            <p>© 2026 SnackFlow MES. All rights reserved.</p>
            <p>This is an automated message, please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink)
    {
        var subject = "Reset Your SnackFlow MES Password";
        var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #0F1C26 0%, #1a2f3d 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .logo {{ font-size: 28px; font-weight: bold; margin-bottom: 10px; }}
        .logo-accent {{ color: #F6C000; }}
        .content {{ background: #ffffff; padding: 30px; border: 1px solid #e5e7eb; border-top: none; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #E87722; color: white; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: 600; }}
        .footer {{ text-align: center; margin-top: 30px; color: #6b7280; font-size: 14px; }}
        .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 15px; margin: 20px 0; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>Snack<span class='logo-accent'>Flow</span> MES</div>
            <p style='margin: 0; font-size: 16px;'>Manufacturing Execution System</p>
        </div>
        <div class='content'>
            <h2 style='color: #0F1C26; margin-top: 0;'>Password Reset Request</h2>
            <p>Hello {fullName},</p>
            <p>We received a request to reset your password for your SnackFlow MES account.</p>
            
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>

            <p style='color: #6b7280; font-size: 14px;'>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; background: #f3f4f6; padding: 10px; border-radius: 4px; font-size: 12px;'>{resetLink}</p>

            <div class='warning'>
                <p style='margin: 0;'><strong>⚠️ Security Notice:</strong></p>
                <p style='margin: 5px 0 0 0;'>This link will expire in 1 hour. If you didn't request this password reset, please ignore this email or contact your administrator.</p>
            </div>
        </div>
        <div class='footer'>
            <p>© 2026 SnackFlow MES. All rights reserved.</p>
            <p>This is an automated message, please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(toEmail, subject, body, isHtml: true);
    }
}
