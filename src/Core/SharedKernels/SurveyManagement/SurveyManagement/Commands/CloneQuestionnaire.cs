using System;

namespace WB.Core.SharedKernels.SurveyManagement.Commands
{
    public class CloneQuestionnaire : QuestionnaireCommand
    {
        public CloneQuestionnaire(Guid questionnaireId, long questionnaireVersion, string newTitle, bool censusMode, Guid userId)
            : base(Guid.NewGuid(), questionnaireId)
        {
            this.QuestionnaireVersion = questionnaireVersion;
            this.NewTitle = newTitle;
            this.CensusMode = censusMode;
            this.UserId = userId;
        }

        public long QuestionnaireVersion { get; }
        public string NewTitle { get; }
        public bool CensusMode { get; }
        public Guid UserId { get; }
    }
}