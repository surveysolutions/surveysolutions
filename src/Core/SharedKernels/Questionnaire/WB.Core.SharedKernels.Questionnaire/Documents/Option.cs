using System;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Option
    {
        public Option() {}

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

        public string Value { get; set; }

        public string Title { get; set; }

        public string ParentValue { get; set; }
    }
}