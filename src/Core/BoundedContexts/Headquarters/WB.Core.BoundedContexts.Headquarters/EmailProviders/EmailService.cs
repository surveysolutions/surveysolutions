using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using SendGrid;
using SendGrid.Helpers.Mail;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using AmazonContent = Amazon.SimpleEmail.Model.Content;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public class EmailService : IEmailService
    {
        private readonly IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage;
        private readonly ISerializer serializer; 

        public EmailService(IPlainKeyValueStorage<EmailProviderSettings> emailProviderSettingsStorage, ISerializer serializer)
        {
            this.emailProviderSettingsStorage = emailProviderSettingsStorage;
            this.serializer = serializer;
        }

        public async Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody)
        {
            EmailProviderSettings settings = emailProviderSettingsStorage.GetById(AppSetting.EmailProviderSettings);
            if (!IsConfigured())
                throw new Exception("Email provider was not set up properly");

            switch (settings.Provider)
            {
                case EmailProvider.Amazon:
                    return await SendEmailWithAmazon(to, subject, htmlBody, textBody, settings);
                case EmailProvider.SendGrid:
                    return await SendEmailWithSendGrid(to, subject, htmlBody, textBody, settings);
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

        public async Task<string> SendEmailWithSendGrid(string to, string subject, string htmlBody, string textBody, ISendGridEmailSettings settings)
        {
            var client = new SendGridClient(settings.SendGridApiKey);
            var msg = new SendGridMessage
            {
                From = new EmailAddress(settings.SenderAddress),
                Subject = subject,
                PlainTextContent = textBody,
                HtmlContent = htmlBody
            };

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

            string body = await response.Body.ReadAsStringAsync();
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

        public async Task<string> SendEmailWithAmazon(string to, string subject, string htmlBody, string textBody, IAmazonEmailSettings settings)
        {
            var credentials = new BasicAWSCredentials(settings.AwsAccessKeyId, settings.AwsSecretAccessKey);
            using (var client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.USEast1))
            {
                var sendRequest = new SendEmailRequest
                {
                    Source = settings.SenderAddress,
                    Destination = new Destination
                    {
                        ToAddresses = new List<string> { to }
                    },
                    Message = new Message
                    {
                        Subject = new AmazonContent(subject),
                        Body = new Body
                        {
                            Html = new AmazonContent
                            {
                                Charset = "UTF-8",
                                Data = htmlBody
                            },
                            Text = new AmazonContent
                            {
                                Charset = "UTF-8",
                                Data = textBody
                            }
                        }
                    }
                };

                if (!string.IsNullOrWhiteSpace(settings.ReplyAddress))
                    sendRequest.ReplyToAddresses = new List<string>() {settings.ReplyAddress};

                try
                {
                    var response = await client.SendEmailAsync(sendRequest).ConfigureAwait(false);
                    return response.MessageId;
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
        }
    }

    public interface IEmailService
    {
        Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody);
        bool IsConfigured();
        ISenderInformation GetSenderInfo();
    }
}
