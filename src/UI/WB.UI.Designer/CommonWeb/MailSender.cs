using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace WB.UI.Designer.CommonWeb
{
    public class MailSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string From { get; set; }
        public bool EnableSSL { get; set; }
    }

    public class MailSender : IEmailSender
    {
        private readonly IOptions<MailSettings> settings;

        public MailSender(IOptions<MailSettings> settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var config = this.settings.Value;
            
            using (var client = new SmtpClient())
            {
                client.Host = config.Host;
                client.Port = config.Port;
                client.Credentials = new NetworkCredential(
                    config.Username,
                    config.Password);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                var message = new MailMessage(
                    to: email,
                    from: config.From,
                    subject: subject,
                    body: htmlMessage
                )
                {
                    IsBodyHtml = true
                };

                client.EnableSsl = config.EnableSSL;

                await client.SendMailAsync(message);
            }
        }
    }
}
