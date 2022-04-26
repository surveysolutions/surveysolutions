using System.Collections.Generic;

namespace WB.Services.Infrastructure.Logging
{
    public class SlackAttachmentsDto
    {
        public class Message
        {
            public List<Attachment> attachments { get; set; } = new List<Attachment>();

            public Attachment AddAttachment()
            {
                var attachment = new Attachment();
                attachments.Add(attachment);
                return attachment;
            }
        }

        public class Attachment
        {
            public string? fallback { get; set; }
            public string? color { get; set; }
            public string? pretext { get; set; }
            public string? author_name { get; set; }
            public string? title { get; set; }
            public string? text { get; set; }
            public List<string> mrkdwn_in { get; set; } = new List<string>();
            public List<Field> fields { get; set; } = new List<Field>();
            public string? footer { get; set; }
            public int ts { get; set; }

            public void AddField(string? name, string? value, bool @short = true) => fields.Add(new Field
            {
                title = name, value = value, @short = @short
            });
        }

        public class Field
        {
            public string? title { get; set; }
            public string? value { get; set; }
            public bool @short { get; set; } = true;
        }
    }
}
