using System.Collections.Generic;
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

        public SlackSink(string webHook, LogEventLevel level)
        {
            this.http = new HttpClient();
            this.webHook = webHook;
            this.level = level;
        }

        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level >= level)
            {
                var fields = new List<object>();

                void AddProp(string name, string title = null)
                {
                    title = title ?? name;
                    if (logEvent.Properties.ContainsKey(name))
                    {
                        fields.Add(new
                        {
                            title, value = logEvent.Properties[name].ToString()
                        });
                    }
                }

                AddProp("tenantName", "Tenant");
                AddProp("jobId", "Job");
                AddProp("Host");
                AddProp("VersionInfo");

                if (logEvent.Exception != null)
                {
                    foreach (var key in logEvent.Exception.Data.Keys)
                    {
                        fields.Add(new
                        {
                            title = key,
                            value = logEvent.Exception.Data[key]
                        });
                    }

                    fields.Add(new
                    {
                        title = "StackTrace",
                        value = logEvent.Exception.ToStringDemystified()
                    });
                }

                http.PostAsync(this.webHook,
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        attachments = new object[]
                        {
                            new
                            {
                                pretext = logEvent.RenderMessage(),
                                fallback = logEvent.RenderMessage(),
                                fields
                            }
                        }
                    }), Encoding.UTF8, "application/json"));
            }
        }
    }
}
