using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateTextListQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateTextListQuestion(Guid questionnaireId, 
            Guid questionId, 
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            int? maxAnswerCount,
            QuestionScope scope,
            List<ValidationCondition> validationConditions)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters,
                validationConditions: validationConditions)
        {
            this.MaxAnswerCount = maxAnswerCount;
            this.Scope = scope;
        }

        public int? MaxAnswerCount { get; private set; }

        public QuestionScope Scope { get; set; }
    }
}
