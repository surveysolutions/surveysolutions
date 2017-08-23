using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewCommentedStatus
    {
        public InterviewCommentedStatus()
        {
        }

        public InterviewCommentedStatus(
            Guid id,
            Guid statusChangeOriginatorId, 
            Guid? supervisorId,
            Guid? interviewerId,
            InterviewExportedAction status, 
            DateTime timestamp, 
            string comment, 
            string statusChangeOriginatorName,
            UserRoles statusChangeOriginatorRole,
            TimeSpan? timeSpanWithPreviousStatus,
            string supervisorName,
            string interviewerName)
        {
            this.Id = id;
            this.StatusChangeOriginatorId = statusChangeOriginatorId;
            this.SupervisorId = supervisorId;
            this.InterviewerId = interviewerId;
            this.Status = status;
            this.Timestamp = timestamp;
            this.Comment = comment;
            this.StatusChangeOriginatorName = statusChangeOriginatorName;
            this.StatusChangeOriginatorRole = statusChangeOriginatorRole;
            this.TimeSpanWithPreviousStatus = timeSpanWithPreviousStatus;
            this.SupervisorName = supervisorName;
            this.InterviewerName = interviewerName;
        }
        public virtual Guid Id { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual string SupervisorName { get; set; }
        public virtual Guid? InterviewerId { get; set; }
        public virtual string InterviewerName { get; set; }
        public virtual Guid StatusChangeOriginatorId { get; set; }
        public virtual string StatusChangeOriginatorName { get; set; }
        public virtual UserRoles StatusChangeOriginatorRole { get; set; }
        public virtual InterviewExportedAction Status { get; set; }
        public virtual DateTime Timestamp { get; set; }

        public virtual TimeSpan? TimeSpanWithPreviousStatus
        {
            get => TimespanWithPreviousStatusLong.HasValue ? new TimeSpan(TimespanWithPreviousStatusLong.Value) : (TimeSpan?)null;
            set => this.TimespanWithPreviousStatusLong = value?.Ticks;
        }

        public virtual long? TimespanWithPreviousStatusLong { get; protected set; }
        public virtual string Comment { get; set; }

        public virtual InterviewStatuses InterviewStatuses { get; set; }
    }
}