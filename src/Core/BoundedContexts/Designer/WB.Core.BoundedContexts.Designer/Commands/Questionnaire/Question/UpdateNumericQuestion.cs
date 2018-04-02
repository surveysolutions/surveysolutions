using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateNumericQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateNumericQuestion(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            bool isPreFilled,
            QuestionScope scope, 
            bool isInteger, 
            bool useFormatting,
            int? countOfDecimalPlaces,
            List<ValidationCondition> validationConditions,
            Option[] options)
            : base(
                 responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                 commonQuestionParameters: commonQuestionParameters,
                 validationConditions: validationConditions)
        {
            this.IsInteger = isInteger;
            this.Properties.UseFormatting = useFormatting;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
            Options = options;
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
        }

        public bool IsInteger { get; private set; }

        public int? CountOfDecimalPlaces { get; private set; }

        public Option[] Options { get; }

        public QuestionScope Scope { get; set; }

        public bool IsPreFilled { get; set; }
    }
}
