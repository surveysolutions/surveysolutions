using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable
{
    [Serializable]
    public class DeleteVariable : QuestionnaireEntityCommand
    {
        public DeleteVariable(Guid questionnaireId, Guid responsibleId, Guid entityId)
            : base(questionnaireId, responsibleId, entityId)
        {
        }
    }
}