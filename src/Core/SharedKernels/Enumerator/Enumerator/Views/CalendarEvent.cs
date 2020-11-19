using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class CalendarEvent : IPlainStorageEntity<Guid>
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        //[AutoIncrement, Unique, NotNull]
        //public int? Order { get; set; }
        public DateTimeOffset Start { get; set; }
        public string StartTimezone { get; set; }
        public string Comment { get; set; }
        public Guid UserId { get; set; }
        public Guid? InterviewId  { get; set; }
        public int AssignmentId { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSynchronized { get; set; }
        public Guid LastEventId { get; set; }
        public string InterviewKey { get; set; }
    }
}
