using System.Net;
using System.Net.Mail;

namespace MVC_Tiraz.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("omarr712007@gmail.com", "fccm odsn yegr csrr")
            };

            return client.SendMailAsync(
                new MailMessage(from: "omarr712007@gmail.com", to: email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                });
        }
    }
}
