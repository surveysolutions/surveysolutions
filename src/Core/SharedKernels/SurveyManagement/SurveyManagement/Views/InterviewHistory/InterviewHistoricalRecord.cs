using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory
{
    public class InterviewHistoricalRecord : IView
    {
        public InterviewHistoricalRecord(Guid interviewId,InterviewHistoricalAction action, Guid originatorId, Dictionary<string, string> parameters, DateTime timestamp)
        {
            this.InterviewId = interviewId;
            this.Action = action;
            this.OriginatorId = originatorId;
            this.Parameters = parameters;
            this.Timestamp = timestamp;
        }
        public Guid InterviewId { get; private set; }
        public InterviewHistoricalAction Action { get; private set; }
        public Guid OriginatorId { get; private set; }
        public Dictionary<string, string> Parameters { get; private set; }
        public DateTime Timestamp { get; private set; }
    }

    public enum InterviewHistoricalAction
    {
        SupervisorAssigned,
        InterviewerAssigned,
        AnswerSet,
        CommentSet,
        Completed,
        Restarted,
        ApproveBySupervisor,
        ApproveByHeadquarter,
        RejectedBySupervisor,
        RejectedByHeadquarter
    }
}
