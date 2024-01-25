using System;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Option
    {
        public Option() { }

        public Option(decimal? value, string title)
        {
            this.Value = value;
            this.Title = title;
            this.ParentValue = null;
        }

        public Option(string value, string title)
        {
            this.Value = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var val) ? val : null;
            this.Title = title;
            this.ParentValue = null;
        }

        public Option(decimal? value, string title, decimal? parentValue) 
            : this(value, title)
        {
            this.ParentValue = parentValue;
        }
        
        public Option(decimal? value, string title, decimal? parentValue, string? attachmentName)
            : this(value, title, parentValue)
        {
            this.AttachmentName = attachmentName;
        }

        public decimal? Value { get; set; }

        public string Title { get; set; } = String.Empty;

        public decimal? ParentValue { get; set; }
        
        public string? AttachmentName { get; set; }
    }
}
