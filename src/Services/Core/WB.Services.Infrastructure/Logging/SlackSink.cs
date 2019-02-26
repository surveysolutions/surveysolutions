using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
                var message = new Message();
                var blocks = message.Blocks;
                
                var fields = new FieldsList();

                blocks.Add(new Section(logEvent.RenderMessage()));
                blocks.Add(new Divider());
                blocks.Add(fields);

                AddPropertyToField("tenantName", "Tenant");
                AddPropertyToField("jobId", "Job");

                if (logEvent.Exception != null)
                {
                    foreach (var key in logEvent.Exception.Data.Keys)
                    {
                        fields.Add(key.ToString(), logEvent.Exception.Data[key]);
                    }

                    message.Attachments.Add(new Attachment
                    {
                        text = $"```\r\n{logEvent.Exception.ToStringDemystified()}\r\n```"
                    });
                }
                
                blocks.Add(new Context()
                {
                    Elements =
                    {
                        new TextItem($"[{logEvent.Properties["Host"].ToString()}]"),
                        new TextItem(workerId),
                        new TextItem(logEvent.Properties["VersionInfo"].ToString())
                    }
                });

                var messageText = JsonConvert.SerializeObject(message);

                File.AppendAllLines("debug.hook", new List<string>{messageText});
                http.PostAsync(this.webHook, new StringContent(messageText,  Encoding.UTF8, "application/json"));

                void AddPropertyToField(string name, string title = null)
                {
                    title = title ?? name;
                    if (logEvent.Properties.ContainsKey(name))
                    {
                        fields.Add(title, logEvent.Properties[name].ToString());
                    }
                }
            }
        }

        public abstract class Block
        {
            [JsonProperty("type")]
            public abstract string Type { get; }
        }

        public class Divider : Block
        {
            public override string Type { get; } = "divider";
        }

        public class Section : Block
        {
            public Section(string text)
            {
                Text = new TextItem(text);
            }

            public override string Type { get; } = "section";

            [JsonProperty("text")]
            public TextItem Text { get; set; }
        }

        public class Attachment
        {
            public string mrkdwn_in { get; } = "text";
            public string text { get; set; }
        }

        public class Message
        {
            [JsonProperty("blocks")]
            public List<Block> Blocks { get; set; } = new List<Block>();

            [JsonProperty("attachments")]
            public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        }

        public class FieldsList : Block
        {
            public override string Type { get; } = "section";

            [JsonProperty("fields")]
            public List<TextItem> Fields { get; set; } = new List<TextItem>();

            public void Add(string message)
            {
                Fields.Add(new TextItem(message));
            }

            public void Add(string key, object value)
            {
                Fields.Add(new TextItem($"*{key}*: {value}"));
            }
        }

        public class Context : Block
        {
            public override string Type { get; } = "context";

            [JsonProperty("elements")]
            public List<TextItem> Elements { get; set; } = new List<TextItem>();

            public void Add(string item)
            {
                Elements.Add(new TextItem(item));
            }
        }

        public class TextItem
        {
            public TextItem(string text)
            {
                Text = text;
            }

            [JsonProperty("type")]
            public string Type { get; set; } = "mrkdwn";

            [JsonProperty("text")]
            public string Text { get; set; }
        }
    }
}
