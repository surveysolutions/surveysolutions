using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Documents
{
    public class LocationDocument
    {
        public LocationDocument()
        {
            CreationDate = DateTime.UtcNow;
        }

        public string Id { get; set; }

        public string Location
        { get; set; }

        public DateTime CreationDate
        { get; set; }
    }
}
