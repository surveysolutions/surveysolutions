using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base
{
    public abstract class FullQuestionDataCommand : QuestionCommand
    {
        protected FullQuestionDataCommand(Guid questionnaireId, Guid questionId,
            string title, QuestionType type, string alias, bool isMandatory, bool isFeatured, bool isHeaderOfPropagatableGroup,
            QuestionScope scope, string condition, string validationExpression, string validationMessage, string instructions,
            Option[] options, Order optionsOrder, int? maxValue, Guid[] triggedGroupIds)
            : base(questionnaireId, questionId)
        {
            this.Title = title;
            this.Type = type;
            this.Alias = alias;
            this.IsMandatory = isMandatory;
            this.IsFeatured = isFeatured;
            this.IsHeaderOfPropagatableGroup = isHeaderOfPropagatableGroup;
            this.Scope = scope;
            this.Condition = condition;
            this.ValidationExpression = validationExpression;
            this.ValidationMessage = validationMessage;
            this.Instructions = instructions;
            this.Options = options;
            this.OptionsOrder = optionsOrder;
            this.MaxValue = maxValue;
            this.TriggedGroupIds = triggedGroupIds;
        }

        public string Title { get; private set; }

        public QuestionType Type { get; private set; }

        public string Alias { get; private set; }

        public bool IsMandatory { get; private set; }

        public bool IsFeatured { get; private set; }

        public bool IsHeaderOfPropagatableGroup { get; private set; }

        public QuestionScope Scope { get; private set; }

        public string Condition { get; set; }

        public string ValidationExpression { get; set; }

        public string ValidationMessage { get; private set; }

        public string Instructions { get; private set; }

        public Option[] Options { get; private set; }

        public Order OptionsOrder { get; private set; }

        public int? MaxValue { get; private set; }

        public Guid[] TriggedGroupIds { get; private set; }
    }
}