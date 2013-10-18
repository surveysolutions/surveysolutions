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
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneNumericQuestion")]
    public class CloneNumericQuestionCommand : AbstractNumericQuestionCommand
    {
        public CloneNumericQuestionCommand(Guid questionnaireId, Guid questionId, Guid groupId, Guid sourceQuestionId, int targetIndex,
            string title, bool isAutopropagating, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions, int? maxValue,
            Guid[] triggeredGroupIds, Guid responsibleId,
            bool isInteger, int? countOfDecimalPlaces)
            : base(questionnaireId, questionId, title, isAutopropagating, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup,
                scope, condition, validationExpression, validationMessage, instructions, responsibleId, maxValue, triggeredGroupIds,
                isInteger, countOfDecimalPlaces)
        {
            this.GroupId = groupId;
            this.SourceQuestionId = sourceQuestionId;
            this.TargetIndex = targetIndex;
        }

        public Guid GroupId { get; private set; }
        public Guid SourceQuestionId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}
