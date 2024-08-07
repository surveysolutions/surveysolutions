#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class EmailService : IEmailService
    {
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly Dictionary<EmailProvider, IEmailService> providers;

        public EmailService(IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage, 
            ISerializer serializer)
        {
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            
            this.providers = new Dictionary<EmailProvider, IEmailService>()
            {
                {EmailProvider.Amazon, new AmazonEmailService(emailProviderSettingsStorage)},
                {EmailProvider.SendGrid, new SendGridEmailService(emailProviderSettingsStorage, serializer)},
                {EmailProvider.Smtp, new SmtpEmailService(emailProviderSettingsStorage)}
            };
        }

        public Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment>? attachments)
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null || !IsConfigured())
                throw new Exception("Email provider was not set up properly");

            if (!providers.TryGetValue(settings.Provider, out var emailService))
                throw new Exception("Email provider wasn't set up");
            
            return emailService.SendEmailAsync(to, subject, htmlBody, textBody, attachments);
        }

        public bool IsConfigured()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                return false;

            if (!providers.TryGetValue(settings.Provider, out var emailService))
                return false;

            return emailService.IsConfigured();
        }

        public ISenderInformation GetSenderInfo()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);

            if (settings == null || !providers.TryGetValue(settings.Provider, out var emailService))
                throw new Exception("Email provider wasn't set up");

            return emailService.GetSenderInfo();
        }
    }
}
