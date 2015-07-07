using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewActionExportView
    {
        public InterviewActionExportView(string interviewId, InterviewExportedAction action, string originator, DateTime timestamp, string role)
        {
            this.Role = role;
            this.InterviewId = interviewId;
            this.Action = action;
            this.Originator = originator;
            this.Timestamp = timestamp;
        }
        public string InterviewId { get; private set; }
        public InterviewExportedAction Action { get; private set; }
        public string Originator { get; private set; }
        public string Role { get; private set; }
        public DateTime Timestamp { get; private set; }
    }

    public enum InterviewExportedAction
    {
        SupervisorAssigned,
        InterviewerAssigned,
        FirstAnswerSet,
        Completed,
        Restarted,
        ApprovedBySupervisor,
        ApprovedByHeadquarter,
        RejectedBySupervisor,
        RejectedByHeadquarter,
        Deleted,
        Restored
    }

    public static class InterviewExportedActionUtils
    {
        public static InterviewStatus? ConvertToInterviewStatus(this InterviewExportedAction action)
        {
            switch (action)
            {
                case InterviewExportedAction.SupervisorAssigned:
                    return InterviewStatus.SupervisorAssigned;
                case InterviewExportedAction.InterviewerAssigned:
                    return InterviewStatus.InterviewerAssigned;
                case InterviewExportedAction.FirstAnswerSet:
                    return null;
                case InterviewExportedAction.Completed:
                    return InterviewStatus.Completed;
                case InterviewExportedAction.Restarted:
                    return InterviewStatus.Restarted;
                case InterviewExportedAction.ApprovedBySupervisor:
                    return InterviewStatus.ApprovedBySupervisor;
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return InterviewStatus.ApprovedByHeadquarters;
                case InterviewExportedAction.RejectedBySupervisor:
                    return InterviewStatus.RejectedBySupervisor;
                case InterviewExportedAction.RejectedByHeadquarter:
                    return InterviewStatus.RejectedByHeadquarters;
                case InterviewExportedAction.Deleted:
                    return InterviewStatus.Deleted;
                case InterviewExportedAction.Restored:
                    return InterviewStatus.Restored;
            }
            return null;
        }
    }
}
