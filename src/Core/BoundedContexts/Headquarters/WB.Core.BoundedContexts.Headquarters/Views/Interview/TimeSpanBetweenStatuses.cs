using System;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class TimeSpanBetweenStatuses
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
        public virtual int Id { get; set; }
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
    }
}