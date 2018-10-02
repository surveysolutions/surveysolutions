using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class RevertVersionQuestionnaire : QuestionnaireCommand
    {
        public Guid HistoryReferenceId { get;  }

        public RevertVersionQuestionnaire(Guid questionnaireId, Guid historyReferenceId, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.HistoryReferenceId = historyReferenceId;
        }
    }
}
