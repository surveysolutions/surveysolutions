using System;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEvent : IReadSideRepositoryEntity
    {
        public CalendarEvent()
        {
        }

        public CalendarEvent(Guid publicKey, DateTimeOffset start, string comment, 
            Guid? interviewId, int assignmentId, bool isCompleted, DateTimeOffset updateDate,
            Guid userId, string userName)
        {
            PublicKey = publicKey;
            Start = start;
            Comment = comment;
            InterviewId = interviewId;
            AssignmentId = assignmentId;
            IsCompleted = isCompleted;
            UserId = userId;
            UserName = userName;
        }

        public virtual Guid PublicKey { get; set; }

        public virtual DateTimeOffset Start { set; get; }

        public virtual string Comment { get; set; }
        
        public virtual Guid? InterviewId { get; set; }
        
        public virtual int AssignmentId { get; set; }
        
        public virtual bool IsCompleted { get; set; }
        
        public virtual DateTimeOffset UpdateDate { set; get; }
        
        public virtual Guid UserId { get; set; }
        public virtual string UserName { get; set; }
    }
}
