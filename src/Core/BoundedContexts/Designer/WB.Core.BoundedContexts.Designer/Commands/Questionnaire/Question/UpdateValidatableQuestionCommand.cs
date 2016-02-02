using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    public class UpdateValidatableQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateValidatableQuestionCommand(Guid responsibleId, Guid questionnaireId, Guid questionId, string title, string variableName, string variableLabel, string enablementCondition, string instructions, List<ValidationCondition> validationConditions)
            : base(responsibleId, questionnaireId, questionId, title, variableName, variableLabel, enablementCondition, instructions)
        {
            this.ValidationConditions = validationConditions;
        }

        public List<ValidationCondition> ValidationConditions { get; set; }
    }
}