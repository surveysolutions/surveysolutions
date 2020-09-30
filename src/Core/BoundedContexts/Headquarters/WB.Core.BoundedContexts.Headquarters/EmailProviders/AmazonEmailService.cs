#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using MimeKit;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class AmazonEmailService : IEmailService
    {
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;

        public AmazonEmailService(IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage)
        {
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
        }
        
        /*public async Task<bool> CanSendEmailAsync()
        {
            EmailProviderSettings settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (!IsConfigured())
                throw new Exception("Email provider was not set up properly");

            var credentials = new BasicAWSCredentials(settings.AwsAccessKeyId, settings.AwsSecretAccessKey);
            var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion);
            using var client = new AmazonSimpleEmailServiceClient(credentials, regionEndpoint);
            var response = await client.GetSendQuotaAsync();
            response.

                return await SendEmailWithAmazon(to, subject, htmlBody, textBody, attachments, settings).ConfigureAwait(false);

        }*/


        public async Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment>? attachments)
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                throw new Exception("Email provider was not set up properly");

            var credentials = new BasicAWSCredentials(settings.AwsAccessKeyId, settings.AwsSecretAccessKey);
            var regionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion);
            using var client = new AmazonSimpleEmailServiceClient(credentials, regionEndpoint);
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(string.Empty, settings.SenderAddress));
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

            await using var messageStream = new MemoryStream();
            await message.WriteToAsync(messageStream);
            
            var sendRequest = new SendRawEmailRequest
            {
                RawMessage = new RawMessage(messageStream),
            };

            try
            {
                var response = await client.SendRawEmailAsync(sendRequest).ConfigureAwait(false);
                //if (response.HttpStatusCode == HttpStatusCode.OK)
                    return response.MessageId;
                
                
            }
            /*catch (AmazonServiceException ae)
            {
                ae.Message == "Maximum sending rate exceeded.";
            }*/
            catch (AggregateException ae)
            {
                throw new EmailServiceException(to, HttpStatusCode.Accepted, ae, 
                    ae.UnwrapAllInnerExceptions().Select(x => x.Message).ToArray());
            }
            catch (Exception ex)
            {
                throw new EmailServiceException(to, HttpStatusCode.Accepted, ex, ex.Message);
            }
        }

        public bool IsConfigured()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                return false;

            return !string.IsNullOrWhiteSpace(settings.AwsAccessKeyId) &&
                   !string.IsNullOrWhiteSpace(settings.AwsSecretAccessKey);
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