using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace WebAppAuthorization.Services.Email
{
    public class EmailSender : IEmailSender
    {
        public string _email = "почта"; 
        public string _password = "пароль";

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                using var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_email, _password)
                };

                var mailMessage = new MailMessage(_email, email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                };

                // await client.SendMailAsync(mailMessage); - отключено
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Письмо не было отправлено, ошибка: {ex.Message}");
            }
        }
    }
}