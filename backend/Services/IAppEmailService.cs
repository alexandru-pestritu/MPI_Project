namespace backend.Services;

/// <summary>
/// Defines application-specific email services, such as sending verification and password reset emails.
/// </summary>
public interface IAppEmailService
{
    /// <summary>
    /// Sends an account verification email to a user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="verificationLink">The verification link to be included in the email.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendVerificationEmailAsync(string toEmail, string verificationLink);

    /// <summary>
    /// Sends a password reset email to a user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="resetLink">The password reset link to be included in the email.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}