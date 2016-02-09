using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateQRBarcodeQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateQRBarcodeQuestionCommand(Guid questionnaireId, 
            Guid questionId, 
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            string validationExpression,
            string validationMessage,
            QuestionScope scope)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters)
        {
            this.ValidationMessage = CommandUtils.SanitizeHtml(validationMessage, removeAllTags: true);
            this.ValidationExpression = validationExpression;
            this.Scope = scope;
        }
        public string ValidationMessage { get; set; }

        public QuestionScope Scope { get; set; }

        public string ValidationExpression { get; set; }
    }
}
