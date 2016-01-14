using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class EventView : IPlainStorageEntity
    {
        public int OID { get; set; }
        public string Id { get; set; }
        public Guid EventId { get; set; }
        public Guid EventSourceId { get; set; }
        public int EventSequence { get; set; }
        public DateTime DateTimeUtc { get; set; }
        public string JsonEvent { get; set; }
    }
}