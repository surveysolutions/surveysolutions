using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.DataExport
{
    public class InterviewActionExportView
    {
        public InterviewActionExportView(string interviewId, InterviewExportedAction action, string originator, DateTime timestamp)
        {
            this.InterviewId = interviewId;
            this.Action = action;
            this.Originator = originator;
            this.Timestamp = timestamp;
        }
        public string InterviewId { get; private set; }
        public InterviewExportedAction Action { get; private set; }
        public string Originator { get; private set; }
        public DateTime Timestamp { get; private set; }
    }

    public enum InterviewExportedAction
    {
        SupervisorAssigned,
        InterviewerAssigned,
        FirstAnswerSet,
        Completed,
        Restarted,
        ApproveBySupervisor,
        ApproveByHeadquarter,
        RejectedBySupervisor,
        RejectedByHeadquarter,
    }
}
