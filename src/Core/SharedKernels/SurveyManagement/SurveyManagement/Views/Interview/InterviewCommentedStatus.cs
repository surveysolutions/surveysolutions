using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
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
            Id = id;
            StatusChangeOriginatorId = statusChangeOriginatorId;
            SupervisorId = supervisorId;
            InterviewerId = interviewerId;
            Status = status;
            Timestamp = timestamp;
            Comment = comment;
            StatusChangeOriginatorName = statusChangeOriginatorName;
            StatusChangeOriginatorRole = statusChangeOriginatorRole;
            TimeSpanWithPreviousStatus = timeSpanWithPreviousStatus;
            SupervisorName = supervisorName;
            InterviewerName = interviewerName;
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
        public virtual TimeSpan? TimeSpanWithPreviousStatus { get; set; }
        public virtual string Comment { get; set; }

        public virtual InterviewStatuses InterviewStatuses { get; set; }
    }
}