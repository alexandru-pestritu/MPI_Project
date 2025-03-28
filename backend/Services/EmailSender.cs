using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace backend.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true)
        {
            var gmailUser = _configuration["Gmail:User"];
            var gmailPassword = _configuration["Gmail:AppPassword"];

            if (string.IsNullOrEmpty(gmailUser) || string.IsNullOrEmpty(gmailPassword))
            {
                throw new ArgumentException("SMTP credentials are missing in the configuration.");
            }

            var mail = new MailMessage
            {
                From = new MailAddress(gmailUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };
            mail.To.Add(new MailAddress(to));

            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.Credentials = new NetworkCredential(gmailUser, gmailPassword);
                    smtpClient.EnableSsl = true;

                    await smtpClient.SendMailAsync(mail);
                }
            }
            catch (SmtpException ex)
            {
               
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw new InvalidOperationException("An error occurred while sending the email.", ex);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
            finally
            {
                mail.Dispose(); 
            }
        }
    }
}
