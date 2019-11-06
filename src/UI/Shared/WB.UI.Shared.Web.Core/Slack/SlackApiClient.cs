using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.Infrastructure.Versions;
using WB.Infrastructure.Native;
using WB.Infrastructure.Native.Logging.Slack;
using WB.UI.Shared.Web.Settings;

namespace WB.UI.Shared.Web.Slack
{
    public class SlackApiClient : ISlackApiClient
    {
        private readonly IProductVersion productVersion;
        private readonly ISlackMessageThrottler throttler;
        private readonly SlackApiConfiguration config = null;
        private readonly HttpClient httpClient = null;
        private readonly Dictionary<FatalExceptionType, SlackChannelConfiguration> channels;
        private readonly string tenantName;

        const string slackApiUri = "https://slack.com/api/chat.postMessage";

        public SlackApiClient(ISettingsProvider settingsProvider, IProductVersion productVersion, ISlackMessageThrottler throttler)
        {
            this.productVersion = productVersion;
            this.throttler = throttler;
            this.tenantName = settingsProvider.AppSettings["Storage.S3.Prefix"];

            if (settingsProvider.TryGetSection<SlackApiConfiguration>("slack", out var section))
            {
                if (string.IsNullOrWhiteSpace(section.OuathToken)) return;

                this.config = section;
                this.httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.OuathToken);
                this.channels = config.Channels.ToDictionary(c => c.Type);
            }
        }

        public async Task SendMessageAsync(SlackFatalMessage message)
        {
            if (config == null) return;

            if (!channels.TryGetValue(message.Type, out var channel))
            {
                if (channels.ContainsKey(FatalExceptionType.None))
                {
                    channel = channels[FatalExceptionType.None];
                }
                else
                {
                    // no channel configured, skipping
                    return;
                }
            }

            await throttler.Throttle(message, config.Throttle, async () =>
            {
                var payload = GetSlackPayload(message, channel);
                

                if (message.Type == FatalExceptionType.HqExportServiceUnavailable)
                {
                    payload.attachments.FirstOrDefault()
                        ?.AddField("Export Service",
                            System.Configuration.ConfigurationManager.AppSettings["Export.ServiceUrl"], true);
                }

                var reply = await SendAsync(slackApiUri, payload);

                if (message.Exception != null)
                {
                    var details = GetExceptionDetails(message, reply);
                    reply = await SendAsync(slackApiUri, details);

                    var data = GetExceptionData(message, reply);
                    await SendAsync(slackApiUri, data);
                }
            });

            async Task<SlackAttachmentsDto.Reply> SendAsync<TReq>(string uri, TReq payload)
            {
                var response = await httpClient.PostAsJsonAsync(uri, payload);
                var responseText = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SlackAttachmentsDto.Reply>(responseText);
            }
        }

        private SlackAttachmentsDto.Message GetSlackPayload(SlackFatalMessage message, SlackChannelConfiguration channel)
        {
            var slackMessage = new SlackAttachmentsDto.Message();

            var attachment = slackMessage.AddAttachment();
            attachment.AddField("Tenant", tenantName);
            attachment.title = message.Message;
            attachment.author_name = message.Exception?.Message;
            attachment.color = message.Color.ToString().ToLower();
            slackMessage.channel = channel.Name;
            attachment.footer = $@"[{Environment.MachineName}] :office: {productVersion.ToString()} `{ System.Configuration.ConfigurationManager.AppSettings["BaseUrl"]}`";
            return slackMessage;
        }

        private SlackAttachmentsDto.Message GetExceptionData(SlackFatalMessage message,
            SlackAttachmentsDto.Reply replyTo)
        {
            var slackMessage = new SlackAttachmentsDto.Message();
            var attachment = slackMessage.AddAttachment();
            
            slackMessage.channel = replyTo.channel;
            slackMessage.thread_ts = replyTo.ts;

            foreach (DictionaryEntry entry in message.Exception.Data)
            {
                attachment.AddField(entry.Key.ToString(), entry.Value.ToString(), true);
            }

            return slackMessage;
        }

        private SlackAttachmentsDto.Message GetExceptionDetails(SlackFatalMessage message,
            SlackAttachmentsDto.Reply replyTo)
        {
            var slackMessage = new SlackAttachmentsDto.Message();
            var attachment = slackMessage.AddAttachment();
            attachment.text = message.Exception.StackTrace.ToString();
            slackMessage.channel = replyTo.channel;
            slackMessage.thread_ts = replyTo.ts;

            return slackMessage;
        }
    }
}
