using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Views.Event
{
    public class EventViewFactory : IViewFactory<EventViewInputModel, EventView>
    {
        private IDocumentSession documentSession;

        public EventViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }

        public EventView Load(EventViewInputModel input)
        {
            var doc = documentSession.Load<EventDocument>(input.PublickKey);
            throw new NotImplementedException();
        }
    }
}
