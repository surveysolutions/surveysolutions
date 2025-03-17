using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory
{
    public class InterviewHistoryView : IView
    {
        public InterviewHistoryView(Guid interviewId, List<InterviewHistoricalRecordView> records, Guid questionnaireId, long questionnaireVersion)
        {
            this.QuestionnaireVersion = questionnaireVersion;
            this.QuestionnaireId = questionnaireId;
            this.InterviewId = interviewId;
            this.Records = records;
        }

        public Guid InterviewId { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }
        public List<InterviewHistoricalRecordView> Records { get; private set; }
        
        private HashSet<InterviewHistoricalAction> ReducedEvents = new()
        {
            InterviewHistoricalAction.QuestionDeclaredInvalid,
            InterviewHistoricalAction.QuestionDeclaredValid,
            InterviewHistoricalAction.QuestionDisabled,
            InterviewHistoricalAction.QuestionEnabled,
            InterviewHistoricalAction.GroupEnabled,
            InterviewHistoricalAction.GroupDisabled,
            InterviewHistoricalAction.VariableDisabled,
            InterviewHistoricalAction.VariableEnabled,
        };

        public void ReduceActions()
        {
            Records = Records.Where(r => !ReducedEvents.Contains(r.Action)).ToList();
        }
    }
}
