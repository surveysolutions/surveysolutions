﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
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

        public Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody)
        {
            if (string.IsNullOrWhiteSpace(settings.EmailFolder))
                throw new ArgumentException();

            var directory = settings.EmailFolder.StartsWith("~")
                ? HttpContext.Current.Server.MapPath(settings.EmailFolder)
                : Path.GetFullPath(settings.EmailFolder);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var guid = Guid.NewGuid();
            File.WriteAllText(Path.Combine(directory, guid + ".html"), htmlBody);
            File.WriteAllText(Path.Combine(directory, guid + ".txt"), textBody);
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
