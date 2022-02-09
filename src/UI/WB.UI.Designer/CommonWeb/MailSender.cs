using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace WB.UI.Designer.CommonWeb
{
    public class MailSettings
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public bool EnableSSL { get; set; }
        public string PickupFolder { get; set; } = string.Empty;
        public bool UsePickupFolder { get; set; } = false;
    }

    public class MailSender : IEmailSender
    {
        private readonly IOptions<MailSettings> settings;
        private readonly IWebHostEnvironment env;

        public MailSender(IOptions<MailSettings> settings, IWebHostEnvironment env)
        {            
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.env = env;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(settings.Value.Host))
            {
                return;
            }

            var config = this.settings.Value;
            
            using (var client = new SmtpClient())
            {
                client.Host = config.Host;
                client.Port = config.Port;
                client.Credentials = new NetworkCredential(
                    config.Username,
                    config.Password);

                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = config.EnableSSL;

                if (this.settings.Value.UsePickupFolder)
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = this.settings.Value.PickupFolder;

                    if (client.PickupDirectoryLocation.StartsWith("~"))
                    {
                        client.PickupDirectoryLocation = client.PickupDirectoryLocation.Replace("~", this.env.ContentRootPath);
                        if (!System.IO.Directory.Exists(client.PickupDirectoryLocation))
                        {
                            System.IO.Directory.CreateDirectory(client.PickupDirectoryLocation);
                        }
                    }
                    client.EnableSsl = false;
                }
                
                var message = new MailMessage(
                    to: email,
                    from: config.From,
                    subject: subject,
                    body: htmlMessage
                )
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
            }
        }
    }
}
