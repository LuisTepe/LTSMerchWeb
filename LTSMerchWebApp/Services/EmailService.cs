using System.Net;
using System.Net.Mail;

namespace LTSMerchWebApp.Services
{
    public class EmailService
    {
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER");
            var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT"));
            var senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL");
            var senderPassword = Environment.GetEnvironmentVariable("SENDER_PASSWORD");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, "LTS Merch Store"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
