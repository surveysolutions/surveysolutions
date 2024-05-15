using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.CriticalRules
{
    [Serializable]
    public class AddCriticalRule : QuestionnaireCommand
    {
        public AddCriticalRule(Guid questionnaireId, Guid id, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }
    }
}
