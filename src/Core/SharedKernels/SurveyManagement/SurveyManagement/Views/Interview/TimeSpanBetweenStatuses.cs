using System;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
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
            SupervisorId = supervisorId;
            InterviewerId = interviewerId;
            BeginStatus = beginStatus;
            EndStatus = endStatus;
            EndStatusTimestamp = endStatusTimestamp;
            TimeSpan = timeSpan;
            SupervisorName = supervisorName;
            InterviewerName = interviewerName;
        }
        public virtual int Id { get; set; }
        public virtual Guid? SupervisorId { get; set; }
        public virtual string SupervisorName { get; set; }
        public virtual Guid? InterviewerId { get; set; }
        public virtual string InterviewerName { get; set; }
        public virtual InterviewExportedAction BeginStatus { get; set; }
        public virtual InterviewExportedAction EndStatus { get; set; }
        public virtual DateTime EndStatusTimestamp { get; set; }
        public virtual TimeSpan TimeSpan { get; set; }

        public virtual InterviewStatusTimeSpans InterviewStatusTimeSpans { get; set; }
    }
}