using System;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Option
    {
        public Option() { }

        public Option(int? value, string title)
        {
            this.Value = value;
            this.Title = title;
            this.ParentValue = null;
        }

        public Option(string value, string title)
        {
            this.Value = int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) ? val : null;
            this.Title = title;
            this.ParentValue = null;
        }

        public Option(int? value, string title, int? parentValue) 
            : this(value, title)
        {
            this.ParentValue = parentValue;
        }
        
        public Option(int? value, string title, int? parentValue, string? attachmentName)
            : this(value, title, parentValue)
        {
            this.AttachmentName = attachmentName;
        }

        public int? Value { get; set; }

        public string Title { get; set; } = String.Empty;

        public int? ParentValue { get; set; }
        
        public string? AttachmentName { get; set; }
    }
}
