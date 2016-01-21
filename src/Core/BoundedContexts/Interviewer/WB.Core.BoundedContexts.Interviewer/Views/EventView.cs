using System;
using SQLite.Net.Attributes;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class EventView : IPlainStorageEntity
    {
        [PrimaryKey, AutoIncrement]
        public int OID { get; set; }
        public string Id { get; set; }
        public Guid EventId { get; set; }
        [Unique]
        public Guid EventSourceId { get; set; }
        [Unique]
        public int EventSequence { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public string JsonEvent { get; set; }
    }
}