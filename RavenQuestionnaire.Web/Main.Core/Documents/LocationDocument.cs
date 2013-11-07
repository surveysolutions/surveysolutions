using System;

namespace Main.Core.Documents
{
    public class LocationDocument
    {
        public LocationDocument()
        {
            this.CreationDate = DateTime.UtcNow;
        }

        public DateTime CreationDate { get; set; }

        public Guid Id { get; set; }

        public string Location { get; set; }
    }
}