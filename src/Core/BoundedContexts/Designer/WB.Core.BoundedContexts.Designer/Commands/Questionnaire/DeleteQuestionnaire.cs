using System;
using Ncqrs.Commanding;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class DeleteQuestionnaire : CommandBase
    {
        public DeleteQuestionnaire(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        public Guid QuestionnaireId { get; private set; }
    }
}