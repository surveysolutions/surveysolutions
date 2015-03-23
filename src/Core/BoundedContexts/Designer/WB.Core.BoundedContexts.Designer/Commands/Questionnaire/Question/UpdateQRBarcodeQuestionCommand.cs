using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateQRBarcodeQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateQRBarcodeQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName,
            string variableLabel, bool isMandatory,
            string enablementCondition, string instructions, Guid responsibleId,
            string validationExpression,
            string validationMessage)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition,
                instructions: instructions, variableLabel: variableLabel)
        {
            this.ValidationMessage = CommandUtils.SanitizeHtml(validationMessage, removeAllTags: true);
            this.ValidationExpression = validationExpression;
        }
        public string ValidationMessage { get; set; }

        public string ValidationExpression { get; set; }
    }
}
