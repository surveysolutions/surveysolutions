using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class AbstractNumericQuestionCommand : AbstractQuestionCommand
    {
        protected AbstractNumericQuestionCommand(Guid questionnaireId, Guid questionId,
            string title, bool isAutopropagating, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Guid responsibleId, int? maxValue,
            Guid[] triggeredGroupIds,
            bool isInteger, int? countOfDecimalPlaces)
            : base(questionnaireId, questionId, title, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup,
                scope, condition, validationExpression, validationMessage, instructions, responsibleId)
        {
            this.MaxValue = maxValue;
            this.TriggeredGroupIds = triggeredGroupIds;
            this.IsInteger = isInteger;
            this.CountOfDecimalPlaces = countOfDecimalPlaces;
            this.IsAutopropagating = isAutopropagating;
        }

        public bool IsAutopropagating { get; set; }

        public int? MaxValue { get; private set; }

        public Guid[] TriggeredGroupIds { get; private set; }

        public bool IsInteger { get; private set; }

        public int? CountOfDecimalPlaces { get; private set; }
    }
}
