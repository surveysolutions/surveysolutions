using System;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.Infrastructure.Domain;
using WB.ServicesIntegration.Export;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class TimeSpanBetweenStatuses : EntityBase<int>
    {
        public TimeSpanBetweenStatuses()
        {
        }

        public TimeSpanBetweenStatuses(
            Guid? supervisorId,
            Guid? interviewerId,
            InterviewExportedAction beginStatus,
            InterviewExportedAction endStatus,
            DateTime endStatusTimestamp,
            TimeSpan timeSpan,
            string supervisorName,
            string interviewerName)
        {
            this.SupervisorId = supervisorId;
            this.InterviewerId = interviewerId;
            this.BeginStatus = beginStatus;
            this.EndStatus = endStatus;
            this.EndStatusTimestamp = endStatusTimestamp;
            this.TimeSpan = timeSpan;
            this.SupervisorName = supervisorName;
            this.InterviewerName = interviewerName;
        }
        public virtual Guid? SupervisorId { get; set; }
        public virtual string SupervisorName { get; set; }
        public virtual Guid? InterviewerId { get; set; }
        public virtual string InterviewerName { get; set; }
        public virtual InterviewExportedAction BeginStatus { get; set; }
        public virtual InterviewExportedAction EndStatus { get; set; }
        public virtual DateTime EndStatusTimestamp { get; set; }
        public virtual TimeSpan TimeSpan
        {
            get => new TimeSpan(this.TimeSpanLong);
            set => this.TimeSpanLong = value.Ticks;
        }
        public virtual long TimeSpanLong { get; protected set; }

        public virtual InterviewSummary InterviewSummary { get; set; }
        
        protected bool SignatureEquals(TimeSpanBetweenStatuses other)
        {
            return base.Equals(other) && SupervisorId.Equals(other.SupervisorId) && InterviewerId.Equals(other.InterviewerId) && EndStatus == other.EndStatus && TimeSpanLong == other.TimeSpanLong;
        }

        protected override bool SignatureEquals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return SignatureEquals((TimeSpanBetweenStatuses) obj);
        }

        protected override int GetSignatureHashCode()
        {
            unchecked
            {
                int hashCode = BaseGetHashCode();
                hashCode = (hashCode * 397) ^ SupervisorId.GetHashCode();
                hashCode = (hashCode * 397) ^ InterviewerId.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) EndStatus;
                hashCode = (hashCode * 397) ^ TimeSpanLong.GetHashCode();
                return hashCode;
            }
        }
    }
}
