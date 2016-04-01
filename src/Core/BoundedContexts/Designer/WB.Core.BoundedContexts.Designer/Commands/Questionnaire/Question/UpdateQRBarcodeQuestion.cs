using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateQRBarcodeQuestion : UpdateValidatableQuestionCommand
    {
        public UpdateQRBarcodeQuestion(Guid questionnaireId, 
            Guid questionId, 
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            string validationExpression,
            string validationMessage,
            QuestionScope scope,
            List<ValidationCondition> validationConditions)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters,
                
                validationConditions: validationConditions)
        {
            this.Scope = scope;
        }
        public QuestionScope Scope { get; set; }
    }
}
