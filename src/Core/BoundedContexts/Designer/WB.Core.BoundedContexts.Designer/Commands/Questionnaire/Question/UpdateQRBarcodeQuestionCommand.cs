using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateQRBarcodeQuestionCommand : UpdateValidatableQuestionCommand
    {
        public UpdateQRBarcodeQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName,
            string variableLabel,
            string enablementCondition, string instructions, Guid responsibleId,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            List<ValidationCondition> validationConditions)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, enablementCondition: enablementCondition,
                instructions: instructions, variableLabel: variableLabel,
                validationConditions: validationConditions)
        {
            this.Scope = scope;
        }
        public QuestionScope Scope { get; set; }
    }
}
