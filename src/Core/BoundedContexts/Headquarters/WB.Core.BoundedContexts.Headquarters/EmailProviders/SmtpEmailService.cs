#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;

        public SmtpEmailService(IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage)
        {
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
        }
        
        public async Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment>? attachments)
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                throw new Exception("Email provider was not set up properly");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(settings.SenderName ?? string.Empty, settings.SenderAddress));
            message.To.Add(new MailboxAddress(string.Empty, to));
            message.Subject = subject;
            
            var body = new BodyBuilder()
            {
                HtmlBody = htmlBody,
                TextBody = textBody,
            };

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var bytes = attachment.Content;
                    var attachmentEntity = body.Attachments.Add(attachment.Filename, bytes, ContentType.Parse(attachment.ContentType));
                    attachmentEntity.ContentDisposition = attachment.Disposition == EmailAttachmentDisposition.Inline 
                        ? new ContentDisposition(ContentDisposition.Inline)
                        : new ContentDisposition(ContentDisposition.Attachment);
                    attachmentEntity.ContentId = attachment.ContentId ?? Guid.NewGuid().ToString();
                }
            }
            
            message.Body = body.ToMessageBody();

            using (var client = new SmtpClient())
            {
                SecureSocketOptions tls = settings.SmtpTlsEncryption ? SecureSocketOptions.Auto : SecureSocketOptions.None;

                try
                {
                    await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, tls);

                    if (settings.SmtpAuthentication)
                    {
                        await client.AuthenticateAsync(settings.SmtpUsername, settings.SmtpPassword);
                    }

                    await client.SendAsync(message);
                    return message.MessageId;
                }
                catch (AggregateException ae)
                {
                    throw new EmailServiceException(to, HttpStatusCode.Accepted, ae,
                        ae.UnwrapAllInnerExceptions().Select(x => x.Message).ToArray());
                }
                catch (Exception ex)
                {
                    throw new EmailServiceException(to, HttpStatusCode.Accepted, ex, ex.Message);
                }
                finally
                {
                    client.Disconnect(true);
                }
            }
        }

        public bool IsConfigured()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                return false;

            return !string.IsNullOrWhiteSpace(settings.SmtpHost) &&
                   (!settings.SmtpAuthentication || (!string.IsNullOrWhiteSpace(settings.SmtpUsername) && !string.IsNullOrWhiteSpace(settings.SmtpPassword)));
        }

        public ISenderInformation GetSenderInfo()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                throw new Exception("Email provider was not set up properly");
            return settings;
        }
    }
}
