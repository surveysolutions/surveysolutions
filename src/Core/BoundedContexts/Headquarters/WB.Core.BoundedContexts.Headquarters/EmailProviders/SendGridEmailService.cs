#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class SendGridEmailService : IEmailService
    {
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly ISerializer serializer;

        public SendGridEmailService(
            IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage, 
            ISerializer serializer)
        {
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.serializer = serializer;
        }
        
        private class SendGridResponseErrors
        {
            public SendGridResponseError[]? Errors { get; set; }
        }

        private class SendGridResponseError
        {
            public string? Message { get; set; }
            public string? Help { get; set; }
            public string? Field { get; set; }
        }

        public async Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment>? attachments)
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                throw new Exception("Email provider was not set up properly");
            
            var client = new SendGridClient(settings.SendGridApiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(settings.SenderAddress),
                Subject = subject,
                PlainTextContent = textBody,
                HtmlContent = htmlBody,
            };

            if (attachments != null)
            {
                msg.Attachments = attachments.Select(a =>
                    new Attachment()
                    {
                        ContentId = a.ContentId ?? Guid.NewGuid().ToString(),
                        Disposition = a.Disposition == EmailAttachmentDisposition.Inline ? "inline" : "attachment",
                        Filename = a.Filename,
                        Content = Convert.ToBase64String(a.Content),
                        Type = a.ContentType,
                    }).ToList();
            }

            if(!string.IsNullOrWhiteSpace(settings.ReplyAddress))
                msg.ReplyTo = new EmailAddress(settings.ReplyAddress);

            msg.AddTo(new EmailAddress(to));
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NoContent)
            {
                var headers = response.DeserializeResponseHeaders(response.Headers);
                var messageIdHeader = headers.FirstOrDefault(x => x.Key.Equals("X-Message-Id", StringComparison.InvariantCultureIgnoreCase));
                return messageIdHeader.Value;
            }

            string body = await response.Body.ReadAsStringAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(body))
            {
                var responseErrors = serializer.Deserialize<SendGridResponseErrors>(body);
                if (responseErrors != null)
                {
                    var errors = responseErrors.Errors!.Select(x => $"{x.Message} For more information go to: {x.Help}").ToArray();
                    throw new EmailServiceException(to, response.StatusCode, null, errors);
                }
            }

            throw new EmailServiceException(to, response.StatusCode);
        }

        public bool IsConfigured()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                return false;

            return !string.IsNullOrWhiteSpace(settings.SendGridApiKey);
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
