using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class Location: IEntity<LocationDocument>
    {
        private LocationDocument innerDocument;

        public string LocationId { get { return innerDocument.Id; } }

        public Location(string title)
        {
            innerDocument = new LocationDocument() { Location = title };
        }
        public Location(LocationDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }
        public void UpdateLocation(string location)
        {
            this.innerDocument.Location = location;
        }

        public LocationDocument GetInnerDocument()
        {
            return this.innerDocument;
        }
    }
}
