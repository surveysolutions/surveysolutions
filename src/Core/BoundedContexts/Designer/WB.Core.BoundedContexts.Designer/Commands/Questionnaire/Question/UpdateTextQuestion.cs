using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateTextQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateTextQuestion(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            string mask,
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
            this.Mask = mask;
        }

        public QuestionScope Scope { get; set; }

        public string Mask { get; set; }

        public bool IsPreFilled { get; set; }
    }
}