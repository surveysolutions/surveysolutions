using System;
using System.Diagnostics;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.Domain;
using WB.ServicesIntegration.Export;
using UserRoles = Main.Core.Entities.SubEntities.UserRoles;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    [DebuggerDisplay("{Status}:{Position} - {InterviewSummary.InterviewId}")]
    public class InterviewCommentedStatus : EntityBase<Guid>
    {
        public InterviewCommentedStatus()
        {
        }
        
        public InterviewCommentedStatus(Guid id,
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
            string interviewerName, 
            int position,
            InterviewSummary summary)
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
            this.InterviewSummary = summary;
            this.Position = position;
        }
        
        public virtual Guid? SupervisorId { get; set; }
        public virtual string SupervisorName { get; set; }
        public virtual Guid? InterviewerId { get; set; }
        public virtual string InterviewerName { get; set; }
        public virtual Guid StatusChangeOriginatorId { get; set; }
        public virtual string StatusChangeOriginatorName { get; set; }
        public virtual UserRoles StatusChangeOriginatorRole { get; set; }
        public virtual InterviewExportedAction Status { get; set; }
        public virtual DateTime Timestamp { get; set; }
        public virtual int Position { get; set; }

        public virtual TimeSpan? TimeSpanWithPreviousStatus
        {
            get => TimespanWithPreviousStatusLong.HasValue ? new TimeSpan(TimespanWithPreviousStatusLong.Value) : (TimeSpan?)null;
            set => this.TimespanWithPreviousStatusLong = value?.Ticks;
        }

        public virtual long? TimespanWithPreviousStatusLong { get; protected set; }
        public virtual string Comment { get; set; }

        public virtual InterviewSummary InterviewSummary { get; set; }

        protected bool SignatureEquals(InterviewCommentedStatus other)
        {
            return base.Equals(other) && SupervisorId.Equals(other.SupervisorId) && InterviewerId.Equals(other.InterviewerId) && StatusChangeOriginatorId.Equals(other.StatusChangeOriginatorId) && Status == other.Status && Timestamp.Equals(other.Timestamp) && Position == other.Position && TimespanWithPreviousStatusLong == other.TimespanWithPreviousStatusLong;
        }

        protected override bool SignatureEquals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return SignatureEquals((InterviewCommentedStatus) obj);
        }

        protected override int GetSignatureHashCode()
        {
            unchecked
            {
                int hashCode = BaseGetHashCode();
                hashCode = (hashCode * 397) ^ SupervisorId.GetHashCode();
                hashCode = (hashCode * 397) ^ InterviewerId.GetHashCode();
                hashCode = (hashCode * 397) ^ StatusChangeOriginatorId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Status;
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ Position;
                hashCode = (hashCode * 397) ^ TimespanWithPreviousStatusLong.GetHashCode();
                return hashCode;
            }
        }
    }
}
