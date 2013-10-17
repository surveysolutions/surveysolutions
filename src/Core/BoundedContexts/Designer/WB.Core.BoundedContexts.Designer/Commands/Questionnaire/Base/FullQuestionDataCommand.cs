using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullQuestionDataCommand : AbstractQuestionCommand
    {
        protected FullQuestionDataCommand(Guid questionnaireId, Guid questionId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Option[] options, Order optionsOrder, Guid responsibleId, Guid? linkedToQuestionId)
            : base(
                questionnaireId, questionId, title, type, alias, isMandatory, isFeatured, isHeaderOfPropagatableGroup, scope, condition,
                validationExpression, validationMessage, instructions, responsibleId)
        {
            this.Options = options;
            this.OptionsOrder = optionsOrder;
            this.LinkedToQuestionId = linkedToQuestionId;
        }

        public Option[] Options { get; private set; }

        public Order OptionsOrder { get; private set; }

        public Guid? LinkedToQuestionId { get; private set; }
    }
}