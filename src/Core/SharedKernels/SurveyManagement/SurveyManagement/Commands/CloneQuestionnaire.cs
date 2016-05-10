using System;

namespace WB.Core.SharedKernels.SurveyManagement.Commands
{
    public class CloneQuestionnaire : QuestionnaireCommand
    {
        public CloneQuestionnaire(Guid questionnaireId, long questionnaireVersion, string newTitle, Guid userId)
            : base(Guid.NewGuid(), questionnaireId)
        {
            this.QuestionnaireVersion = questionnaireVersion;
            this.NewTitle = newTitle;
            this.UserId = userId;
        }

        public long QuestionnaireVersion { get; }
        public string NewTitle { get; }
        public Guid UserId { get; }
    }
}