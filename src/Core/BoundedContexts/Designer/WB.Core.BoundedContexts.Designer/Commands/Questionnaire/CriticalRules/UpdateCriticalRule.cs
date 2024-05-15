using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.CriticalRules
{
    [Serializable]
    public class UpdateCriticalRule : QuestionnaireCommand
    {
        public UpdateCriticalRule(Guid questionnaireId, Guid id, string message, string expression, string description, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.Id = id;
            this.Message = CommandUtils.SanitizeHtml(message);
            this.Expression = expression;
            this.Description = CommandUtils.SanitizeHtml(description, removeAllTags: true);
        }

        public Guid Id { get; private set; }
        public string Message { get; set; }
        public string Expression { get; set; }
        public string Description { get; set; }
    }
}
