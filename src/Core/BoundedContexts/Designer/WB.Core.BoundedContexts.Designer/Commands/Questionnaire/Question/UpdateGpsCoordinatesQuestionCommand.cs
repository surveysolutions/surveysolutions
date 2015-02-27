using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateGpsCoordinatesQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateGpsCoordinatesQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            string title,
            string variableName, string variableLabel,
            bool isMandatory,
            string enablementCondition,
            string validationExpression,
            string validationMessage,
            string instructions,
            Guid responsibleId,
            QuestionScope scope)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, isMandatory: isMandatory, enablementCondition: enablementCondition, instructions: instructions,variableLabel:variableLabel)
        {
            this.Scope = scope;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
        }

        public string ValidationMessage { get; set; }
        public string ValidationExpression { get; set; }
        public QuestionScope Scope { get; set; }
    }
}