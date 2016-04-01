using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
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
            List<ValidationCondition> validationConditions)
             : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters,
                validationConditions: validationConditions)
        {
            this.IsPreFilled = isPreFilled;
            this.Scope = scope;
        }

        public QuestionScope Scope { get; set; }

        public bool IsPreFilled { get; set; }
    }
}