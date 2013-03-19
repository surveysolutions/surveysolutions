namespace Main.Core.Entities.SubEntities
{
    using System;

    public class Option
    {
        public Option(Guid id, string value, string title)
        {
            this.Id = id;
            this.Value = value;
            this.Title = title;
        }

        public Guid Id { get; set; }

        public string Value { get; set; }

        public string Title { get; set; }
    }
}