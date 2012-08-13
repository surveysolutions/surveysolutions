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
            this.Chunks = new List<ProcessedEventChunk>();
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
        public List<ProcessedEventChunk> Chunks { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EventState Handled { get; set; }
        public SynchronizationType SynckType { get; set; }
    }

    public class ProcessedEventChunk
    {
        public ProcessedEventChunk()
        {
            this.EventChunckPublicKey = Guid.NewGuid();
            this.EventKeys=new List<Guid>();
        }
        public ProcessedEventChunk(IEnumerable<AggregateRootEvent> eventDoc):this()
        {
            
            this.EventKeys = eventDoc.Select(e => e.EventIdentifier).ToList();
            this.Handled = EventState.Initial;
        }

        public Guid EventChunckPublicKey { get; set; }
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

    public enum SynchronizationType
    {
        Push,
        Pull
    }
}
