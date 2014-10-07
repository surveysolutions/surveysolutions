using System;
using System.Globalization;

namespace Main.Core.Entities.SubEntities
{
    public class Option
    {
        public Option() {}

        public Option(Guid id, string value, string title)
        {
            this.Id = id;
            this.Value = value;
            this.Title = title;
            this.ParentValue = null;
        }

        public Option(Guid id, string value, string title, string parentValue) 
            : this(id, value, title)
        {
            this.ParentValue = parentValue;
        }

        public Option(Guid id, string value, string title, decimal? parentValue)
            : this(id, value, title)
        {
            this.ParentValue = parentValue.HasValue? parentValue.Value.ToString(CultureInfo.InvariantCulture) : null;
        }

        public Guid Id { get; set; }

        public string Value { get; set; }

        public string Title { get; set; }

        public string ParentValue { get; set; }
    }
}