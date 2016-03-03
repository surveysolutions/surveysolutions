using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateGpsCoordinatesQuestionCommand : UpdateValidatableQuestionCommand
    {
        public UpdateGpsCoordinatesQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            bool isPreFilled,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            List<ValidationCondition> validationConditions)
            :base ( 
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId,
                commonQuestionParameters: commonQuestionParameters,
                validationConditions: validationConditions)
        {
            this.Scope = scope;
            this.IsPreFilled = isPreFilled;
        }

        public QuestionScope Scope { get; set; }
        public bool IsPreFilled { get; set; }
    }
}