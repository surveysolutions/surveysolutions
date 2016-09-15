using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class DeleteQuestionnaire : QuestionnaireCommand
    {
        public DeleteQuestionnaire(Guid questionnaireId, Guid responsibleId) : base(questionnaireId, responsibleId) { }
    }
}