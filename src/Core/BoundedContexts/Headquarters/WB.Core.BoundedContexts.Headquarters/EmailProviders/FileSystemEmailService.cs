using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class FileSystemEmailServiceSettings
    {
        public FileSystemEmailServiceSettings(bool isEnabled, string emailFolder, string senderAddress, string senderName, string replyAddress, string address)
        {
            IsEnabled = isEnabled;
            EmailFolder = emailFolder;
            SenderAddress = senderAddress;
            SenderName = senderName;
            ReplyAddress = replyAddress;
            Address = address;
        }

        public bool IsEnabled { get; }
        public string EmailFolder { get; }
        public string SenderAddress { get; }
        public string SenderName { get; }
        public string ReplyAddress  { get; }
        public string Address { get; }
    }

    public class FileSystemEmailService : IEmailService
    {
        private readonly FileSystemEmailServiceSettings settings;

        public class FileSystemSenderInformation : ISenderInformation
        {
            public string SenderAddress { get; set; }
            public string SenderName { get; set; }
            public string ReplyAddress { get; set; }
            public string Address { get; set; }
        }

        public FileSystemEmailService(FileSystemEmailServiceSettings settings)
        {
            this.settings = settings;
        }

        public Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment> attachments)
        {
            if (string.IsNullOrWhiteSpace(settings.EmailFolder))
                throw new ArgumentException();

            var directory = Path.GetFullPath(settings.EmailFolder);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var guid = Guid.NewGuid();
            File.WriteAllText(Path.Combine(directory, guid + ".html"), htmlBody);
            File.WriteAllText(Path.Combine(directory, guid + ".txt"), textBody);

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var attachmentPath = Path.Combine(directory, guid.ToString(), attachment.Filename);
                    var bytes = attachment.Content;
                    File.WriteAllBytes(attachmentPath, bytes);
                }
            }
            
            return Task.FromResult(guid.ToString());
        }

        public bool IsConfigured()
        {
            return settings.IsEnabled;
        }

        public ISenderInformation GetSenderInfo()
        {
            return new FileSystemSenderInformation()
            {
                Address = settings.Address,
                ReplyAddress = settings.ReplyAddress,
                SenderAddress = settings.SenderAddress,
                SenderName = settings.SenderName
            };
        }
    }
}
