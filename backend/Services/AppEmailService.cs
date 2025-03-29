namespace backend.Services;

public class AppEmailService : IAppEmailService
{
    private readonly IEmailSender _emailSender;

    public AppEmailService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        var subject = "Email Verification";
        var body = $@"
            <p>Please click the link below to verify your email:</p>
            <p><a href='{verificationLink}'>Verify Now</a></p>";
        await _emailSender.SendEmailAsync(toEmail, subject, body, true);
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Password Reset";
        var body = $@"
            <p>Please click the link below to reset your password:</p>
            <p><a href='{resetLink}'>Reset Password</a></p>";
        return _emailSender.SendEmailAsync(toEmail, subject, body, true);
    }
}