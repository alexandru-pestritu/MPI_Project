namespace backend.Services;

/// <summary>
/// Defines a service for sending emails asynchronously.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email message asynchronously.
    /// </summary>
    /// <param name="to">The recipient's email address.</param>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="body">The body content of the email.</param>
    /// <param name="isBodyHtml">Indicates whether the body content is HTML. Defaults to <c>true</c>.</param>
    /// <returns>A task that represents the asynchronous email send operation.</returns>
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true);
}