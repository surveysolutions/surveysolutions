using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Events;

namespace RavenQuestionnaire.Core.Documents
{
    public class SyncProcessDocument
    {
        public SyncProcessDocument()
        {
            this.AggregateRoots = new List<ProcessedAggregateRoot>();
        }

        public string Id { get; set; }

        public Guid PublicKey
        {
            get { return publicKey; }
            set
            {
                publicKey = value;
                Id = publicKey.ToString();
            }
        }

        private Guid publicKey;
        public List<ProcessedAggregateRoot> AggregateRoots { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class ProcessedAggregateRoot
    {
        public ProcessedAggregateRoot()
        {
            this.EventKeys=new List<Guid>();
        }
        public ProcessedAggregateRoot(AggregateRootEventStream eventDoc)
        {
            this.AggregateRootPublicKey = eventDoc.SourceId;
            this.EventKeys = eventDoc.Events.Select(e => e.EventIdentifier).ToList();
            this.Handled = EventState.Initial;
        }

        public Guid AggregateRootPublicKey { get; set; }
        public List<Guid> EventKeys { get; set; }
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
