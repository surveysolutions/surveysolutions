using System;
using SQLite;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class EventView
    {
        [PrimaryKey]
        public Guid EventId { get; set; }

        [Unique(Name = "Unique AR sequence")]
        [Indexed]
        public Guid EventSourceId { get; set; }

        [Unique(Name = "Unique AR sequence")]
        public int EventSequence { get; set; }

        public Guid? CommitId { get; set; }

        public DateTime DateTimeUtc { get; set; }

        public string JsonEvent { get; set; }

        public string EventType { get; set; }
    }
}