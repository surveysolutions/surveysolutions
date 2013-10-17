using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "UpdateNumericQuestion")]
    public class UpdateNumericQuestionCommand : AbstractNumericQuestionCommand
    {
        public UpdateNumericQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, bool isAutopropagating, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions, int? maxValue,
            Guid[] triggedGroupIds, Guid responsibleId,
            bool isInteger, int? countOfDecimalPlaces)
            : base(questionnaireId, questionId, title, isAutopropagating, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup,
                scope, condition, validationExpression, validationMessage, instructions, responsibleId, maxValue, triggedGroupIds,
                isInteger, countOfDecimalPlaces) {}
    }
}
