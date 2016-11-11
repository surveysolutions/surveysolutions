using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class RestoreVersionQuestionnaire : QuestionnaireCommand
    {
        public Guid HistoryReferanceId { get;  }

        public RestoreVersionQuestionnaire(Guid questionnaireId, Guid historyReferanceId, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.HistoryReferanceId = historyReferanceId;
        }
    }
}