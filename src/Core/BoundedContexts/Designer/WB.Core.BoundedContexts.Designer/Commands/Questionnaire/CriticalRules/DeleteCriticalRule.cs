using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.CriticalRules
{
    [Serializable]
    public class DeleteCriticalRule : QuestionnaireCommand
    {
        public DeleteCriticalRule(Guid questionnaireId, Guid id, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }
    }
}
