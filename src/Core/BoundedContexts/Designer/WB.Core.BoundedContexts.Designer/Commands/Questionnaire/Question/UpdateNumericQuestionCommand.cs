using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateNumericQuestionCommand : UpdateValidatableQuestionCommand
    {
        public UpdateNumericQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            string title,
            string variableName, string variableLabel, 
            bool isPreFilled,
            QuestionScope scope, 
            string enablementCondition, 
            string instructions,
            Guid responsibleId,
            bool isInteger, 
            int? countOfDecimalPlaces,
            List<ValidationCondition> validationConditions)
            : base(
                 responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, title: title,
                 variableName: variableName, enablementCondition: enablementCondition, instructions: instructions, variableLabel:variableLabel,
                 validationConditions: validationConditions)
        {
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
        }

        public bool IsInteger { get; private set; }

        public int? CountOfDecimalPlaces { get; private set; }

        public QuestionScope Scope { get; set; }

        public bool IsPreFilled { get; set; }
    }
}
