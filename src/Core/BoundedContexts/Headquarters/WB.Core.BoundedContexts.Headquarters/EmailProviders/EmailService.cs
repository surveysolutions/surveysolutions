#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Domain;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class EmailService : IEmailService
    {
        private readonly IInScopeExecutor<IPlainKeyValueStorage<EmailProviderSettings>> settingsExecutor;
        private readonly ISerializer serializer;

        public EmailService(IInScopeExecutor<IPlainKeyValueStorage<EmailProviderSettings>> settingsExecutor,
            ISerializer serializer)
        {
            this.settingsExecutor = settingsExecutor;
            this.serializer = serializer;
        }

        public Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment>? attachments)
        {
            var emailService = GetProvider();
            if (emailService == null || !emailService.IsConfigured())
                throw new Exception("Email provider was not set up properly");

            return emailService.SendEmailAsync(to, subject, htmlBody, textBody, attachments);
        }

        public bool IsConfigured() => GetProvider()?.IsConfigured() == true;

        public ISenderInformation GetSenderInfo()
        {
            var emailService = GetProvider() ?? throw new Exception("Email provider wasn't set up");

            return emailService.GetSenderInfo();
        }

        private IEmailService? GetProvider()
        {
            var settings = settingsExecutor.Execute(storage => storage.GetById(AppSetting.EmailProviderSettings));
            if (settings == null)
                return null;

            // Providers read settings again during sending. Never give them database-backed storage:
            // its unit of work would keep a connection checked out throughout the network operation.
            var snapshot = new InMemoryKeyValueStorage<EmailProviderSettings>(
                new Dictionary<string, EmailProviderSettings> { [AppSetting.EmailProviderSettings] = settings });
            return settings.Provider switch
            {
                EmailProvider.Amazon => new AmazonEmailService(snapshot),
                EmailProvider.SendGrid => new SendGridEmailService(snapshot, serializer),
                EmailProvider.Smtp => new SmtpEmailService(snapshot),
                _ => null
            };
        }
    }
}
