using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using System.Collections.Generic;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateDateTimeQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateDateTimeQuestion(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            QuestionScope scope,
            bool isPreFilled,
            List<ValidationCondition> validationConditions,
            bool isTimestamp,
            DateTime? defaultDate)
             : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters,
                validationConditions: validationConditions)
        {
            this.IsTimestamp = isTimestamp;
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
            this.Properties.DefaultDate = defaultDate;
        }

        public QuestionScope Scope { get; set; }

        public bool IsPreFilled { get; set; }

        public bool IsTimestamp { get; set; }
    }
}
