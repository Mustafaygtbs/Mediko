
using System.Net;
using System.Net.Mail;
using MailKit.Net.Smtp;
using Mediko.DataAccess.Interfaces;
using Mediko.Entities;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = System.Net.Mail.SmtpClient;

namespace Mediko.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IConfiguration _config;

        public EmailService(IOptions<EmailSettings> emailSettings, IConfiguration config)
        {
            _emailSettings = emailSettings.Value;
            _config = config;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var port = int.Parse(_config["EmailSettings:Port"]);
            var senderEmail = _config["EmailSettings:SenderEmail"];
            var userName = _config["EmailSettings:UserName"];
            var password = _config["EmailSettings:Password"];

            using (var client = new SmtpClient(smtpServer, port))
            {
                client.Credentials = new NetworkCredential(userName, password);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, "Mediko Sağlık Merkezi"),
                    Subject = subject,
                    Body = htmlBody, 
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
       
       

    }
}
