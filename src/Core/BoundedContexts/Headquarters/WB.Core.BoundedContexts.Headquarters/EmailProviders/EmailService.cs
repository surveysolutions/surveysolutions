using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using AmazonContent = Amazon.SimpleEmail.Model.Content;
using Attachment = SendGrid.Helpers.Mail.Attachment;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class EmailService : IEmailService
    {
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly ISerializer serializer; 

        public EmailService(IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage, 
            ISerializer serializer)
        {
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.serializer = serializer;
        }

        public async Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment> attachments = null)
        {
            EmailProviderSettings settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (!IsConfigured())
                throw new Exception("Email provider was not set up properly");

            switch (settings.Provider)
            {
                case EmailProvider.Amazon:
                    return await SendEmailWithAmazon(to, subject, htmlBody, textBody, attachments, settings).ConfigureAwait(false);
                case EmailProvider.SendGrid:
                    return await SendEmailWithSendGrid(to, subject, htmlBody, textBody, attachments, settings).ConfigureAwait(false);
                default:
                    throw new Exception("Email provider wasn't set up");
            }
        }

        public bool IsConfigured()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (settings == null)
                return false;

            switch (settings.Provider)
            {
                case EmailProvider.None: return false;
                case EmailProvider.Amazon:
                    return !string.IsNullOrWhiteSpace(settings.AwsAccessKeyId) &&
                           !string.IsNullOrWhiteSpace(settings.AwsSecretAccessKey);
                case EmailProvider.SendGrid:
                    return !string.IsNullOrWhiteSpace(settings.SendGridApiKey);
                default:
                    return false;
            }
        }

        public ISenderInformation GetSenderInfo()
        {
            var settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            return settings;
        }

        public async Task<string> SendEmailWithSendGrid(string to, string subject, string htmlBody, string textBody, List<EmailAttachment> attachments, ISendGridEmailSettings settings)
        {
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
                        ContentId = Guid.NewGuid().ToString(),
                        Disposition = "attachment",
                        Filename = a.Filename,
                        Content = a.Base64String,
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
                    var errors = responseErrors.Errors.Select(x => $"{x.Message} For more information go to: {x.Help}").ToArray();
                    throw new EmailServiceException(to, response.StatusCode, null, errors);
                }
            }

            throw new EmailServiceException(to, response.StatusCode);
        }

        private class SendGridResponseErrors
        {
            public SendGridResponseError[] Errors { get; set; }
        }

        private class SendGridResponseError
        {
            public string Message { get; set; }
            public string Help { get; set; }
            public string Field { get; set; }
        }

        public async Task<string> SendEmailWithAmazon(string to, string subject, string htmlBody, string textBody, List<EmailAttachment> attachments, IAmazonEmailSettings settings)
        {
            var credentials = new BasicAWSCredentials(settings.AwsAccessKeyId, settings.AwsSecretAccessKey);
            using var client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.USEast1);
            
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(settings.SenderAddress);
            mailMessage.To.Add(new MailAddress(to));
            mailMessage.Subject = subject;
            mailMessage.Body = htmlBody;
            mailMessage.IsBodyHtml = true;
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody));

            if (!string.IsNullOrEmpty(settings.ReplyAddress))
                mailMessage.ReplyToList.Add(new MailAddress(settings.ReplyAddress));

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    var bytes = Convert.FromBase64String(attachment.Base64String);
                    var ms = new MemoryStream(bytes);
                    var mailAttachment = new System.Net.Mail.Attachment(ms, attachment.Filename, attachment.ContentType);
                    mailMessage.Attachments.Add(mailAttachment);
                }
            }

           
            /*var messageBuilder = new RawMessageBuilder();
            messageBuilder.From = settings.SenderAddress;
            messageBuilder.To = to;
            messageBuilder.ReplyAddress = settings.ReplyAddress;
            messageBuilder.Subject = subject;
            messageBuilder.HtmlBody = htmlBody;
            messageBuilder.TextBody = textBody;
            messageBuilder.Attachments = attachments;
            var rawMessage = messageBuilder.Build();

            var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(rawMessage));
            var sendRequest = new SendRawEmailRequest
            {
                Source = settings.SenderAddress,
                Destinations = new List<string>() { to },
                RawMessage = new RawMessage(messageStream),
            };*/
            
            
            try
            {
                // var response = await client.SendRawEmailAsync(sendRequest).ConfigureAwait(false);
                // return response.MessageId;
                await SendMessageAsync(mailMessage, settings);
                return "1";
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
        }
        
        private async Task SendMessageAsync(MailMessage emailMessage, IAmazonEmailSettings settings)
        {
            using var client = new SmtpClient();
            var credential = new NetworkCredential
            {
                UserName = settings.AwsAccessKeyId,
                Password = settings.AwsSecretAccessKey
            };

            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = credential;
            client.Host = "email-smtp.us-east-1.amazonaws.com";
            client.Port = 587;
            client.EnableSsl = true;

            await client.SendMailAsync(emailMessage);
        }
    }

    public interface IEmailService
    {
        Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment> attachments = null);
        bool IsConfigured();
        ISenderInformation GetSenderInfo();
    }
}
