using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Serilog.Core;
using Serilog.Events;

namespace WB.Services.Infrastructure.Logging
{
    public class SlackSink : ILogEventSink
    {
        private readonly HttpClient http;
        private readonly string webHook;
        private readonly LogEventLevel level;
        private readonly string workerId;

        public SlackSink(string webHook, LogEventLevel level, string workerId)
        {
            this.http = new HttpClient();
            this.webHook = webHook;
            this.level = level;
            this.workerId = workerId;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level >= level)
            {
                SlackAttachmentsDto.Message message = new SlackAttachmentsDto.Message();

                var attachment = message.AddAttachment();

                attachment.author_name = workerId;
                attachment.mrkdwn_in.Add("fields");
                attachment.footer = $"[{logEvent.Properties["Host"].ToString()}] :fixik: {logEvent.Properties["VersionInfo"].ToString()}";
                attachment.title = logEvent.RenderMessage();

                AddPropertyToField("tenantName", "Tenant");
                AddPropertyToField("jobId", "Job");

                if (logEvent.Exception != null)
                {
                    foreach (var key in logEvent.Exception.Data.Keys)
                    {
                        if(key!=null)
                            attachment.AddField(key.ToString(), logEvent.Exception.Data[key]?.ToString());
                    }
                    
                    attachment.AddField("Stack Trace", $"```\r\n{logEvent.Exception.ToStringDemystified()}\r\n```", false);
                }

                http.PostAsync(this.webHook,
                    new StringContent(JsonConvert.SerializeObject(message),
                        Encoding.UTF8, "application/json")).Wait();

                void AddPropertyToField(string name, string? title = null)
                {
                    title = title ?? name;
                    if (logEvent.Properties.ContainsKey(name))
                    {
                        attachment.AddField(title, logEvent.Properties[name].ToString());
                    }
                }
            }
        }
        


    }
}
