using System;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Option
    {
        public Option() { }

        public Option(string value, string title)
        {
            this.Value = value;
            this.Title = title;
            this.ParentValue = null;
        }

        public Option(string value, string title, string parentValue) 
            : this(value, title)
        {
            this.ParentValue = parentValue;
        }

        public Option(string value, string title, decimal? parentValue)
            : this(value, title)
        {
            this.ParentValue = parentValue.HasValue ? parentValue.Value.ToString("G29", CultureInfo.InvariantCulture) : null;
        }
        
        public Option(string value, string title, decimal? parentValue, string? attachmentName)
            : this(value, title, parentValue)
        {
            this.AttachmentName = attachmentName;
        }

        public string Value { get; set; } = String.Empty;

        public string Title { get; set; } = String.Empty;

        public string? ParentValue { get; set; }
        
        public string? AttachmentName { get; set; }
    }
}
