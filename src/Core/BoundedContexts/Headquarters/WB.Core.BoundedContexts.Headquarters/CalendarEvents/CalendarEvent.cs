#nullable enable
using System;
using NodaTime;
using WB.Core.BoundedContexts.Headquarters.Aggregates;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEvent : IReadSideRepositoryEntity
    {
        protected CalendarEvent()
        {
        }

        public CalendarEvent(Guid publicKey, ZonedDateTime start, 
            string comment, Guid? interviewId, string interviewKey, int assignmentId, 
            DateTimeOffset updateDate, Guid userId)
        {
            PublicKey = publicKey;
            Start = start;
            Comment = comment;
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            AssignmentId = assignmentId;
            CreatorUserId = userId;
            UpdateDateUtc = updateDate.UtcDateTime;
        }

        public virtual Guid PublicKey { get; set; }
        public virtual string Comment { get; set; } = String.Empty;
        public virtual Guid? InterviewId { get; set; }
        public virtual string InterviewKey { get; set; } = String.Empty;
        public virtual int AssignmentId { get; set; }
        public virtual DateTime? CompletedAtUtc { get; set; }
        public virtual bool IsCompleted() => CompletedAtUtc != null;
        public virtual DateTime UpdateDateUtc { set; get; }
        public virtual Guid CreatorUserId { get; set; }
        
        public virtual ReadonlyUser? Creator { get; protected set; }

        public virtual DateTime?  DeletedAtUtc { get; set; }
        public virtual bool IsDeleted() => DeletedAtUtc != null;
        public virtual ZonedDateTime Start { get; set; }

        public virtual DateTime StartUtc() => Start.ToDateTimeUtc();
        public virtual string StartTimezone() => Start.Zone.Id;
    }
}
