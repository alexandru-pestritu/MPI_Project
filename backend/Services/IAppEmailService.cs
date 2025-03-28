namespace backend.Services;

public interface IAppEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string verificationLink);
}