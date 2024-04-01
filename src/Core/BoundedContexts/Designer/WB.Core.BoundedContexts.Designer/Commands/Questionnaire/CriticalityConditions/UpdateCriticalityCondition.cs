using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.CriticalityConditions
{
    [Serializable]
    public class UpdateCriticalityCondition : QuestionnaireCommand
    {
        public UpdateCriticalityCondition(Guid questionnaireId, Guid id, string message, string expression, string description, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.Id = id;
            this.Message = message;
            this.Expression = expression;
            this.Description = description;
        }

        public Guid Id { get; private set; }
        public string Message { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
    }
}
