using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullQuestionDataCommand : AbstractQuestionCommand
    {
        protected FullQuestionDataCommand(Guid questionnaireId, Guid questionId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Option[] options, Order optionsOrder, Guid responsibleId, Guid? linkedToQuestionId, bool areAnswersOrdered, int? maxAllowedAnswers)
            : base(
                questionnaireId, questionId, title, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup, scope, condition,
                validationExpression, validationMessage, instructions, responsibleId)
        {
            this.Options = options;
            this.OptionsOrder = optionsOrder;
            this.LinkedToQuestionId = linkedToQuestionId;
            this.Type = type;

            this.AreAnswersOrdered = areAnswersOrdered;
            this.MaxAllowedAnswers = maxAllowedAnswers;

        }

        public QuestionType Type { get; private set; }
        public Option[] Options { get; private set; }
        public Order OptionsOrder { get; private set; }
        public Guid? LinkedToQuestionId { get; private set; }

        public bool AreAnswersOrdered { get; private set; }
        public int? MaxAllowedAnswers { get; private set; }
    }
}