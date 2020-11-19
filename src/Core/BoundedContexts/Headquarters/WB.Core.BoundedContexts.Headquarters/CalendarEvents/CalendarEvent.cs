using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEvent : IReadSideRepositoryEntity
    {
        public CalendarEvent()
        {
        }

        public CalendarEvent(Guid publicKey, DateTime start, string startTimezone, string comment, 
            Guid? interviewId, string interviewKey, int assignmentId, bool isCompleted, 
            DateTimeOffset updateDate, Guid userId, string userName)
        {
            PublicKey = publicKey;
            Start = start;
            StartTimezone = startTimezone;
            Comment = comment;
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            AssignmentId = assignmentId;
            IsCompleted = isCompleted;
            UserId = userId;
            UserName = userName;
            UpdateDate = updateDate.UtcDateTime;
        }

        public virtual Guid PublicKey { get; set; }

        public virtual DateTime Start { set; get; }
        
        public virtual string StartTimezone { set; get; }

        public virtual string Comment { get; set; }
        
        public virtual Guid? InterviewId { get; set; }
        public virtual string InterviewKey { get; set; }
        
        public virtual int AssignmentId { get; set; }
        
        public virtual bool IsCompleted { get; set; }
        
        public virtual DateTime UpdateDate { set; get; }
        
        public virtual Guid UserId { get; set; }
        public virtual string UserName { get; set; }
        
        public virtual bool IsDeleted { get; set; }
    }
}
