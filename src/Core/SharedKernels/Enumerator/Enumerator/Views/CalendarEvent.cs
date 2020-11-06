using System;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class CalendarEvent : IPlainStorageEntity<Guid>
    {
        public Guid Id { get; set; }
        public DateTimeOffset Start { get; set; } 
        public string Comment   { get; set; }
        public Guid UserId { get; set; }
        public Guid? InterviewId  { get; set; }
        public int AssignmentId { get; set; }
        public DateTimeOffset LastUpdateDate  { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSynchronized { get; set; }
    }
}