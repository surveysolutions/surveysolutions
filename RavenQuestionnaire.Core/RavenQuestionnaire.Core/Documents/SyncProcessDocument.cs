using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.Documents
{
    public class SyncProcessDocument
    {
        public SyncProcessDocument()
        {
            this.Events = new List<ProcessedEvent>();
        }

        public string Id { get; set; }

        public Guid PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                this.Id = publicKey.ToString();
            }
        }

        private Guid publicKey;
        public List<ProcessedEvent> Events { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ProcessedEvent
    {
        public ProcessedEvent()
        {
        }
        public ProcessedEvent(EventDocument eventDoc)
        {
            this.Event = eventDoc;
            this.Handled=EventState.Initial;
        }
        public EventDocument Event { get; set; }
        public EventState Handled { get; set; }
    }

    public enum EventState
    {
        Initial,
        InProgress,
        Completed,
        Error
    }
}
