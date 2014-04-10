using System;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Numeric
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "UpdateNumericQuestion")]
    public class UpdateNumericQuestionCommand : AbstractNumericQuestionCommand
    {
        public UpdateNumericQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, bool isAutopropagating, string alias, bool isMandatory, bool isFeatured,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions, int? maxValue,
            Guid[] triggeredGroupIds, Guid responsibleId,
            bool isInteger, int? countOfDecimalPlaces)
            : base(questionnaireId, questionId, title, isAutopropagating, alias, isMandatory, isFeatured,
                scope, condition, validationExpression, validationMessage, instructions, responsibleId, maxValue, triggeredGroupIds,
                isInteger, countOfDecimalPlaces) {}
    }
}
