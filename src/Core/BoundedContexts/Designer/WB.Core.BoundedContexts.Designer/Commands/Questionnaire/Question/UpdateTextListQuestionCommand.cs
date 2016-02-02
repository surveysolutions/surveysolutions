using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateTextListQuestionCommand : UpdateValidatableQuestionCommand
    {
        public UpdateTextListQuestionCommand(Guid questionnaireId, Guid questionId, string title, string variableName, string variableLabel,
            string enablementCondition, string instructions, Guid responsibleId, int? maxAnswerCount,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            List<ValidationCondition> validationConditions)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                variableName: variableName, enablementCondition: enablementCondition, instructions: instructions, variableLabel:variableLabel,
                validationConditions: validationConditions)
        {
            this.MaxAnswerCount = maxAnswerCount;
            this.Scope = scope;
        }

        public int? MaxAnswerCount { get; private set; }

        public QuestionScope Scope { get; set; }
    }
}
