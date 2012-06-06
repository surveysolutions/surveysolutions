using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class SyncProcess : IEntity<SyncProcessDocument>
    {
        public SyncProcess(Guid publicKey)
        {
            this.innerDocument = new SyncProcessDocument();
            this.innerDocument.PublicKey = publicKey;
            this.innerDocument.StartDate = DateTime.UtcNow;
        }
        public SyncProcess(SyncProcessDocument document)
        {
            this.innerDocument = document;
        }
        public void AddEvents(IEnumerable<EventDocument> events)
        {
            if(this.innerDocument.EndDate.HasValue)
                throw new InvalidOperationException("process is finished, events can't be added");
            this.innerDocument.Events =
                events.Select(e => new ProcessedEvent(e)).ToList();
        }
        public void SetEventState(Guid eventKey,EventState state)
        {
            if (this.innerDocument.EndDate.HasValue)
                throw new InvalidOperationException("process is finished, events can't be modifyed");
            var eventDoc = this.innerDocument.Events.FirstOrDefault(e => e.Event.PublicKey == eventKey);
            if(eventDoc==null)
                throw  new ArgumentException("Event wasn't find");
            eventDoc.Handled = state;
        }
        public void EndProcess()
        {
            if (this.innerDocument.EndDate.HasValue)
                throw new InvalidOperationException("process is already finished");
            this.innerDocument.EndDate = DateTime.UtcNow;
        }

        private SyncProcessDocument innerDocument;

        public Guid SynckProcessGuid { get { return innerDocument.PublicKey; } }
        public SyncProcessDocument GetInnerDocument()
        {
            return this.innerDocument;
        }
    }
}
