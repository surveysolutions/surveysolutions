using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class EventRepository : EntityRepository<Event, EventDocument>, IEventRepository
    {
        public EventRepository(IDocumentSession documentSession) : base(documentSession)
        {
        }
        
        protected override Event Create(EventDocument doc)
        {
            return new Event(doc);
        }
    }
}
