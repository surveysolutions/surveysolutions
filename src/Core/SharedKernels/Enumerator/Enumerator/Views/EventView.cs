using System;
using System.ComponentModel;
using SQLite;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class EventView
    {
        [PrimaryKey]
        public Guid EventId { get; set; }

        [Unique(Name = "Unique AR sequence")]
        public Guid EventSourceId { get; set; }

        [Unique(Name = "Unique AR sequence")]
        public int EventSequence { get; set; }

        public Guid? CommitId { get; set; }

        public DateTime DateTimeUtc { get; set; }

        public string JsonEvent { get; set; }

        public string EventType { get; set; }

        public int? ExistsOnHq { get; set; } // library does not support good way of handling default values and bools https://github.com/praeclarum/sqlite-net/issues/326

        public string EncryptedJsonEvent { get; set; }
    }
}
