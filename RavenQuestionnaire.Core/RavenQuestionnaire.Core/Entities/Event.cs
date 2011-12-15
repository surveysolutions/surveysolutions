using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class Event: IEntity<EventDocument>
    {
        private EventDocument innerDocument;
        public EventDocument GetInnerDocument()
        {
            return  innerDocument;
        }

        public Event(EventDocument document)
        {
            innerDocument = document;
        }
    }
}
