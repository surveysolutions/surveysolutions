using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WB.Services.Infrastructure.Logging
{
    public class SlackBlocksBto
    {
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
            public string text { get; set; } = String.Empty;
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
