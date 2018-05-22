using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateAreaQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateAreaQuestion(Guid questionnaireId, 
            Guid questionId, 
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            List<ValidationCondition> validationConditions,
            QuestionScope scope)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters, validationConditions:validationConditions)
        {
            this.Scope = scope;
        }

        public QuestionScope Scope { get; set; }
    }
}
