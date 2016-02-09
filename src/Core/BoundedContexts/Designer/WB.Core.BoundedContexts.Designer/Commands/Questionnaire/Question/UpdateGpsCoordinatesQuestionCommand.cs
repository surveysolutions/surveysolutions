using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateGpsCoordinatesQuestionCommand : AbstractUpdateQuestionCommand
    {
        public UpdateGpsCoordinatesQuestionCommand(
            Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            bool isPreFilled,
            string validationExpression,
            string validationMessage,
            QuestionScope scope)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId,
                  commonQuestionParameters: commonQuestionParameters)
        {
            this.Scope = scope;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.IsPreFilled = isPreFilled;
        }

        public string ValidationMessage { get; set; }
        public string ValidationExpression { get; set; }
        public QuestionScope Scope { get; set; }
        public bool IsPreFilled { get; set; }
    }
}